using System;
using UnityEngine;
using EndlessRunner.Player;
using EndlessRunner.Save;

namespace EndlessRunner.Core
{
    /// <summary>
    /// Core Game States.
    /// </summary>
    public enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        GameOver
    }

    /// <summary>
    /// Coordinates the main loop, handles scoring, combo streaks, run statistics,
    /// and triggers saves upon player crash.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public GameState CurrentState { get; private set; } = GameState.MainMenu;

        [Header("References")]
        [SerializeField] private PlayerController player;

        [Header("Combo Settings")]
        [SerializeField] private float comboWindow = 2.5f; // Duration of active combo streak
        [SerializeField] private int maxComboMultiplier = 5;

        // Current Run counters
        public int CurrentScore => Mathf.RoundToInt(currentScore);
        public int CoinsCollected => coinsCollected;
        public int GemsCollected => gemsCollected;
        public int CurrentCombo => currentCombo;
        public float ComboTimerRatio => Mathf.Clamp01(comboTimer / comboWindow);

        private float currentScore;
        private int coinsCollected;
        private int gemsCollected;
        
        private int currentCombo = 1;
        private float comboTimer;

        // Events for UI managers
        public event Action<GameState> OnGameStateChanged;
        public event Action<int> OnScoreChanged;
        public event Action<int> OnCoinsChanged;
        public event Action<int> OnGemsChanged;
        public event Action<int> OnComboChanged;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            if (player != null)
            {
                player.OnCoinCollected += HandleCoinCollected;
                player.OnGemCollected += HandleGemCollected;
                player.OnDeath += HandlePlayerDeath;
            }

            SetState(GameState.MainMenu);
        }

        private void OnDestroy()
        {
            if (player != null)
            {
                player.OnCoinCollected -= HandleCoinCollected;
                player.OnGemCollected -= HandleGemCollected;
                player.OnDeath -= HandlePlayerDeath;
            }
        }

        private void Update()
        {
            if (CurrentState == GameState.Playing)
            {
                // Score is distance traveled + coin bonuses
                if (player != null)
                {
                    currentScore = player.transform.position.z + (coinsCollected * 15f);
                    OnScoreChanged?.Invoke(CurrentScore);
                }

                // Tick down active combo streaks
                if (currentCombo > 1)
                {
                    comboTimer -= Time.deltaTime;
                    if (comboTimer <= 0f)
                    {
                        currentCombo = 1;
                        OnComboChanged?.Invoke(currentCombo);
                    }
                }
            }
        }

        /// <summary>
        /// Transitions to a new GameState and updates global variables (like timescales).
        /// </summary>
        public void SetState(GameState newState)
        {
            CurrentState = newState;

            switch (newState)
            {
                case GameState.Paused:
                    Time.timeScale = 0f;
                    break;
                default:
                    // Check if slow motion is active in player before restoring scale
                    var pm = player != null ? player.GetComponent<PowerUpManager>() : null;
                    bool isSlowMo = pm != null && pm.IsPowerUpActive(PowerUpType.SlowMotion);
                    Time.timeScale = isSlowMo ? 0.6f : 1f;
                    break;
            }

            OnGameStateChanged?.Invoke(newState);
        }

        /// <summary>
        /// Resets run stats and sets state to playing.
        /// </summary>
        public void StartGame()
        {
            currentScore = 0f;
            coinsCollected = 0;
            gemsCollected = 0;
            currentCombo = 1;
            comboTimer = 0f;

            if (player != null)
            {
                player.transform.position = Vector3.zero;
                player.transform.rotation = Quaternion.identity;
                player.TransitionToState(new RunningState(player));
            }

            SetState(GameState.Playing);

            OnScoreChanged?.Invoke(0);
            OnCoinsChanged?.Invoke(0);
            OnGemsChanged?.Invoke(0);
            OnComboChanged?.Invoke(1);
        }

        public void PauseGame()
        {
            if (CurrentState == GameState.Playing)
            {
                SetState(GameState.Paused);
            }
        }

        public void ResumeGame()
        {
            if (CurrentState == GameState.Paused)
            {
                SetState(GameState.Playing);
            }
        }

        private void HandleCoinCollected()
        {
            if (CurrentState != GameState.Playing) return;

            coinsCollected++;
            
            // Advance combo multiplier
            currentCombo = Mathf.Min(currentCombo + 1, maxComboMultiplier);
            comboTimer = comboWindow;

            OnCoinsChanged?.Invoke(coinsCollected);
            OnComboChanged?.Invoke(currentCombo);
        }

        private void HandleGemCollected()
        {
            if (CurrentState != GameState.Playing) return;

            gemsCollected++;
            OnGemsChanged?.Invoke(gemsCollected);
        }

        private void HandlePlayerDeath()
        {
            if (CurrentState != GameState.Playing) return;

            SetState(GameState.GameOver);
            SaveRunData();
        }

        private void SaveRunData()
        {
            var sd = SaveManager.Instance?.SaveData;
            if (sd == null) return;

            sd.coins += coinsCollected;
            sd.gems += gemsCollected;

            // Leveling XP = Score / 10
            int xpGained = CurrentScore / 10;
            sd.xp += xpGained;

            if (CurrentScore > sd.highScore)
            {
                sd.highScore = CurrentScore;
            }

            SaveManager.Instance.Save();
        }

        /// <summary>
        /// Revives player and consumes one gem.
        /// </summary>
        public void Revive()
        {
            var sd = SaveManager.Instance?.SaveData;
            if (sd != null && sd.gems > 0)
            {
                sd.gems--;
                SaveManager.Instance.Save();

                if (player != null)
                {
                    player.TransitionToState(new RunningState(player));
                }

                SetState(GameState.Playing);
            }
        }

        public int GetPlayerLevel()
        {
            var sd = SaveManager.Instance?.SaveData;
            if (sd == null) return 1;

            // Level Formula = 1 + floor(sqrt(XP) * 0.05)
            return Mathf.FloorToInt(Mathf.Sqrt(sd.xp) * 0.05f) + 1;
        }

        public int GetPlayerXP()
        {
            var sd = SaveManager.Instance?.SaveData;
            return sd != null ? sd.xp : 0;
        }
    }
}

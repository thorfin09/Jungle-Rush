using TMPro;
using UnityEngine;
using UnityEngine.UI;
using EndlessRunner.Core;
using EndlessRunner.Player;
using EndlessRunner.PowerUps;

namespace EndlessRunner.UI
{
    /// <summary>
    /// UI Controller for the active in-game HUD.
    /// Handles scores, combos, coins, gems, and power-up meters.
    /// </summary>
    public class HUDView : View
    {
        [Header("References")]
        [SerializeField] private PlayerController player;

        [Header("Stats Displays")]
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private TMP_Text coinsText;
        [SerializeField] private TMP_Text gemsText;
        
        [Header("Combo Streak HUD")]
        [SerializeField] private GameObject comboPanel;
        [SerializeField] private TMP_Text comboText;
        [SerializeField] private Image comboProgressRing;

        [Header("Power Up Progress Rings")]
        [SerializeField] private GameObject magnetRingObj;
        [SerializeField] private Image magnetRingFill;
        
        [SerializeField] private GameObject boostRingObj;
        [SerializeField] private Image boostRingFill;
        
        [SerializeField] private GameObject shieldRingObj;
        [SerializeField] private Image shieldRingFill;

        [Header("Pause Button")]
        [SerializeField] private Button pauseButton;
        [SerializeField] private View pausePanel;

        private PowerUpManager powerUpManager;

        protected override void Awake()
        {
            base.Awake();
            pauseButton.onClick.AddListener(OnPauseClicked);
        }

        private void Start()
        {
            var gm = GameManager.Instance;
            if (gm != null)
            {
                gm.OnScoreChanged += UpdateScore;
                gm.OnCoinsChanged += UpdateCoins;
                gm.OnGemsChanged += UpdateGems;
                gm.OnComboChanged += UpdateCombo;
            }

            if (player != null)
            {
                powerUpManager = player.GetComponent<PowerUpManager>();
                if (powerUpManager != null)
                {
                    powerUpManager.OnPowerUpStateChanged += HandlePowerUpStateChange;
                    powerUpManager.OnPowerUpTimerUpdated += HandlePowerUpTimerUpdate;
                }
            }

            // Hide powerup indicator overlays by default
            magnetRingObj.SetActive(false);
            boostRingObj.SetActive(false);
            shieldRingObj.SetActive(false);
            comboPanel.SetActive(false);
        }

        private void OnDestroy()
        {
            var gm = GameManager.Instance;
            if (gm != null)
            {
                gm.OnScoreChanged -= UpdateScore;
                gm.OnCoinsChanged -= UpdateCoins;
                gm.OnGemsChanged -= UpdateGems;
                gm.OnComboChanged -= UpdateCombo;
            }

            if (powerUpManager != null)
            {
                powerUpManager.OnPowerUpStateChanged -= HandlePowerUpStateChange;
                powerUpManager.OnPowerUpTimerUpdated -= HandlePowerUpTimerUpdate;
            }
        }

        private void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Playing)
            {
                // Update active combo streak ring fill
                if (GameManager.Instance.CurrentCombo > 1)
                {
                    comboProgressRing.fillAmount = GameManager.Instance.ComboTimerRatio;
                }
            }
        }

        private void UpdateScore(int score)
        {
            scoreText.text = score.ToString("D7"); // Pad score to 7 digits
        }

        private void UpdateCoins(int coins)
        {
            coinsText.text = coins.ToString();
        }

        private void UpdateGems(int gems)
        {
            gemsText.text = gems.ToString();
        }

        private void UpdateCombo(int multiplier)
        {
            if (multiplier > 1)
            {
                comboPanel.SetActive(true);
                comboText.text = $"{multiplier}x";
            }
            else
            {
                comboPanel.SetActive(false);
            }
        }

        private void HandlePowerUpStateChange(PowerUpType type, bool isActive)
        {
            switch (type)
            {
                case PowerUpType.CoinMagnet:
                    magnetRingObj.SetActive(isActive);
                    break;
                case PowerUpType.SpeedBoost:
                    boostRingObj.SetActive(isActive);
                    break;
                case PowerUpType.Shield:
                    shieldRingObj.SetActive(isActive);
                    break;
            }
        }

        private void HandlePowerUpTimerUpdate(PowerUpType type, float progress)
        {
            switch (type)
            {
                case PowerUpType.CoinMagnet:
                    magnetRingFill.fillAmount = progress;
                    break;
                case PowerUpType.SpeedBoost:
                    boostRingFill.fillAmount = progress;
                    break;
                case PowerUpType.Shield:
                    shieldRingFill.fillAmount = progress;
                    break;
            }
        }

        private void OnPauseClicked()
        {
            GameManager.Instance.PauseGame();
            if (pausePanel != null)
            {
                pausePanel.Show();
            }
        }
    }
}

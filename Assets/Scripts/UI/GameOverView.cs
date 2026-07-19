using TMPro;
using UnityEngine;
using UnityEngine.UI;
using EndlessRunner.Core;
using EndlessRunner.Save;

namespace EndlessRunner.UI
{
    /// <summary>
    /// UI Controller for the Game Over / Crash screen.
    /// </summary>
    public class GameOverView : View
    {
        [Header("Statistics Displays")]
        [SerializeField] private TMP_Text finalScoreText;
        [SerializeField] private TMP_Text coinsCollectedText;
        [SerializeField] private TMP_Text gemsCollectedText;
        [SerializeField] private TMP_Text highScoreText;
        
        [Header("Revive System")]
        [SerializeField] private Button reviveButton;
        [SerializeField] private TMP_Text gemCostInfoText;

        [Header("Navigation Buttons")]
        [SerializeField] private Button restartButton;
        [SerializeField] private Button mainMenuButton;

        [Header("Main Menu Link")]
        [SerializeField] private View mainMenuView;

        protected override void Awake()
        {
            base.Awake();
            
            restartButton.onClick.AddListener(OnRestartClicked);
            mainMenuButton.onClick.AddListener(OnMainMenuClicked);
            reviveButton.onClick.AddListener(OnReviveClicked);
        }

        public override void Show()
        {
            base.Show();
            PopulateResults();
        }

        private void PopulateResults()
        {
            var gm = GameManager.Instance;
            var sd = SaveManager.Instance?.SaveData;

            if (gm == null || sd == null) return;

            finalScoreText.text = $"SCORE: {gm.CurrentScore}";
            coinsCollectedText.text = $"+{gm.CoinsCollected} COINS";
            gemsCollectedText.text = $"+{gm.GemsCollected} GEMS";
            highScoreText.text = $"HIGH SCORE: {sd.highScore}";

            // Display revive button only if they have gems to burn
            bool hasGems = sd.gems > 0;
            reviveButton.gameObject.SetActive(hasGems);
            if (hasGems)
            {
                gemCostInfoText.text = $"REVIVE (COSTS 1)\nGEMS HELD: {sd.gems}";
            }
        }

        private void OnRestartClicked()
        {
            Hide();
            GameManager.Instance.StartGame();
        }

        private void OnMainMenuClicked()
        {
            Hide();
            if (mainMenuView != null)
            {
                mainMenuView.Show();
            }
            GameManager.Instance.SetState(GameState.MainMenu);
        }

        private void OnReviveClicked()
        {
            var gm = GameManager.Instance;
            var sd = SaveManager.Instance?.SaveData;

            if (gm != null && sd != null && sd.gems > 0)
            {
                Hide();
                gm.Revive();
            }
        }
    }
}

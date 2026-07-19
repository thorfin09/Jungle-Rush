using TMPro;
using UnityEngine;
using UnityEngine.UI;
using EndlessRunner.Core;
using EndlessRunner.Save;

namespace EndlessRunner.UI
{
    /// <summary>
    /// UI Controller for the Main Menu screen.
    /// </summary>
    public class MainMenuView : View
    {
        [Header("Visual Stats")]
        [SerializeField] private TMP_Text highScoreText;
        [SerializeField] private TMP_Text playerLevelText;
        
        [Header("Menu Buttons")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button shopButton;
        [SerializeField] private Button settingsButton;

        [Header("Linked Panels")]
        [SerializeField] private ShopView shopView;
        [SerializeField] private SettingsView settingsView;

        protected override void Awake()
        {
            base.Awake();
            
            playButton.onClick.AddListener(OnPlayClicked);
            shopButton.onClick.AddListener(OnShopClicked);
            settingsButton.onClick.AddListener(OnSettingsClicked);
        }

        public override void Show()
        {
            base.Show();
            UpdateStats();
        }

        private void UpdateStats()
        {
            if (SaveManager.Instance != null && SaveManager.Instance.SaveData != null)
            {
                highScoreText.text = $"HIGH SCORE: {SaveManager.Instance.SaveData.highScore}";
                playerLevelText.text = $"LEVEL: {GameManager.Instance.GetPlayerLevel()}";
            }
        }

        private void OnPlayClicked()
        {
            Hide();
            GameManager.Instance.StartGame();
        }

        private void OnShopClicked()
        {
            if (shopView != null)
            {
                Hide();
                shopView.Show();
            }
        }

        private void OnSettingsClicked()
        {
            if (settingsView != null)
            {
                settingsView.Show();
            }
        }
    }
}

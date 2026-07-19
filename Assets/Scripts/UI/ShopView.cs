using TMPro;
using UnityEngine;
using UnityEngine.UI;
using EndlessRunner.Save;

namespace EndlessRunner.UI
{
    /// <summary>
    /// UI Controller for the Shop/Upgrades screen.
    /// Manages purchasing upgrades and cosmetic data.
    /// </summary>
    public class ShopView : View
    {
        [Header("Coin Balance Display")]
        [SerializeField] private TMP_Text balanceText;

        [Header("Magnet Upgrades")]
        [SerializeField] private TMP_Text magnetLevelText;
        [SerializeField] private TMP_Text magnetCostText;
        [SerializeField] private Button upgradeMagnetButton;

        [Header("Speed Boost Upgrades")]
        [SerializeField] private TMP_Text boostLevelText;
        [SerializeField] private TMP_Text boostCostText;
        [SerializeField] private Button upgradeBoostButton;

        [Header("Shop Buttons")]
        [SerializeField] private Button closeButton;
        [SerializeField] private View mainMenuView;

        private const int MaxLevel = 5;
        private const int BaseUpgradeCost = 1000;

        protected override void Awake()
        {
            base.Awake();
            
            upgradeMagnetButton.onClick.AddListener(() => TryUpgrade("Magnet"));
            upgradeBoostButton.onClick.AddListener(() => TryUpgrade("SpeedBoost"));
            closeButton.onClick.AddListener(OnCloseClicked);
        }

        public override void Show()
        {
            base.Show();
            RefreshShopUI();
        }

        private void RefreshShopUI()
        {
            var sd = SaveManager.Instance?.SaveData;
            if (sd == null) return;

            balanceText.text = $"COINS: {sd.coins}";

            // Update Magnet UI Info
            int magLvl = sd.GetUpgradeLevel("Magnet");
            magnetLevelText.text = $"LEVEL: {magLvl}/{MaxLevel}";
            if (magLvl < MaxLevel)
            {
                int cost = (magLvl + 1) * BaseUpgradeCost;
                magnetCostText.text = $"{cost} COINS";
                upgradeMagnetButton.interactable = sd.coins >= cost;
            }
            else
            {
                magnetCostText.text = "MAX LEVEL";
                upgradeMagnetButton.interactable = false;
            }

            // Update Speed Boost UI Info
            int bstLvl = sd.GetUpgradeLevel("SpeedBoost");
            boostLevelText.text = $"LEVEL: {bstLvl}/{MaxLevel}";
            if (bstLvl < MaxLevel)
            {
                int cost = (bstLvl + 1) * BaseUpgradeCost;
                boostCostText.text = $"{cost} COINS";
                upgradeBoostButton.interactable = sd.coins >= cost;
            }
            else
            {
                boostCostText.text = "MAX LEVEL";
                upgradeBoostButton.interactable = false;
            }
        }

        private void TryUpgrade(string powerUpId)
        {
            var sd = SaveManager.Instance?.SaveData;
            if (sd == null) return;

            int level = sd.GetUpgradeLevel(powerUpId);
            if (level >= MaxLevel) return;

            int cost = (level + 1) * BaseUpgradeCost;
            if (sd.coins >= cost)
            {
                sd.coins -= cost;
                sd.SetUpgradeLevel(powerUpId, level + 1);

                SaveManager.Instance.Save();
                RefreshShopUI();
            }
        }

        private void OnCloseClicked()
        {
            Hide();
            if (mainMenuView != null)
            {
                mainMenuView.Show();
            }
        }
    }
}

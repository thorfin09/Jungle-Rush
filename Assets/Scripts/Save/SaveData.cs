using System;
using System.Collections.Generic;

namespace EndlessRunner.Save
{
    /// <summary>
    /// Data structure for the local JSON-serialized save file.
    /// Stores scores, currencies, cosmetics, and power-up upgrades.
    /// </summary>
    [Serializable]
    public class SaveData
    {
        public int highScore = 0;
        public int coins = 0;
        public int gems = 0;
        public int xp = 0;
        public string activeSkinId = "Hero_Default";
        public List<string> unlockedSkinIds = new List<string> { "Hero_Default" };
        public List<PowerUpUpgradeEntry> powerUpUpgrades = new List<PowerUpUpgradeEntry>();

        public int GetUpgradeLevel(string powerUpId)
        {
            var entry = powerUpUpgrades.Find(e => e.powerUpId == powerUpId);
            return entry.level; // Defaults to 0 if not found
        }

        public void SetUpgradeLevel(string powerUpId, int level)
        {
            int idx = powerUpUpgrades.FindIndex(e => e.powerUpId == powerUpId);
            if (idx >= 0)
            {
                var entry = powerUpUpgrades[idx];
                entry.level = level;
                powerUpUpgrades[idx] = entry;
            }
            else
            {
                powerUpUpgrades.Add(new PowerUpUpgradeEntry { powerUpId = powerUpId, level = level });
            }
        }
    }

    [Serializable]
    public struct PowerUpUpgradeEntry
    {
        public string powerUpId;
        public int level;
    }
}

using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace EndlessRunner.Save
{
    /// <summary>
    /// Persistent manager for reading, writing, and resetting save files.
    /// Uses basic XOR obfuscation to secure progress from local JSON tampering.
    /// </summary>
    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance { get; private set; }

        public SaveData SaveData { get; private set; }

        private string saveFilePath;
        private const string ObfuscationKey = "JungleRunnerCryptoKey";

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                transform.SetParent(null);
                DontDestroyOnLoad(gameObject);
                
                saveFilePath = Path.Combine(Application.persistentDataPath, "player_progress.json");
                Load();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Serializes progress, obfuscates the string, and writes to disk.
        /// </summary>
        public void Save()
        {
            try
            {
                if (SaveData == null) return;

                string rawJson = JsonUtility.ToJson(SaveData, true);
                string securedJson = Obfuscate(rawJson);
                
                File.WriteAllText(saveFilePath, securedJson);
            }
            catch (Exception ex)
            {
                Debug.LogError($"SaveManager: Save failed - {ex.Message}");
            }
        }

        /// <summary>
        /// Reads, de-obfuscates, and restores progress from disk. Falls back to a clean template.
        /// </summary>
        public void Load()
        {
            try
            {
                if (File.Exists(saveFilePath))
                {
                    string securedJson = File.ReadAllText(saveFilePath);
                    string rawJson = Obfuscate(securedJson);
                    
                    SaveData = JsonUtility.FromJson<SaveData>(rawJson);

                    if (SaveData == null)
                    {
                        ResetProgress();
                    }
                }
                else
                {
                    ResetProgress();
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"SaveManager: Load error (reinitializing) - {ex.Message}");
                ResetProgress();
            }
        }

        /// <summary>
        /// Deletes current progress and resets variables.
        /// </summary>
        public void ResetProgress()
        {
            SaveData = new SaveData();
            Save();
        }

        /// <summary>
        /// XOR cipher to obfuscate JSON payload from general users.
        /// </summary>
        private string Obfuscate(string input)
        {
            var output = new StringBuilder();
            for (int i = 0; i < input.Length; i++)
            {
                output.Append((char)(input[i] ^ ObfuscationKey[i % ObfuscationKey.Length]));
            }
            return output.ToString();
        }
    }
}

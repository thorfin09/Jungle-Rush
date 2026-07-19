using UnityEngine;
using UnityEngine.UI;
using EndlessRunner.Audio;

namespace EndlessRunner.UI
{
    /// <summary>
    /// UI Controller for the Settings/Options screen.
    /// Manages sound and music volume values, persisting settings via PlayerPrefs.
    /// </summary>
    public class SettingsView : View
    {
        [Header("Audio Controllers")]
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private Slider sfxVolumeSlider;
        
        [Header("Settings Buttons")]
        [SerializeField] private Button closeButton;

        protected override void Awake()
        {
            base.Awake();
            
            closeButton.onClick.AddListener(Hide);
            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        }

        public override void Show()
        {
            base.Show();
            
            // Prime sliders from saved values
            musicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVol", 0.8f);
            sfxVolumeSlider.value = PlayerPrefs.GetFloat("SFXVol", 0.8f);
        }

        private void OnMusicVolumeChanged(float volume)
        {
            PlayerPrefs.SetFloat("MusicVol", volume);
            PlayerPrefs.Save();

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetMusicVolume(volume);
            }
        }

        private void OnSFXVolumeChanged(float volume)
        {
            PlayerPrefs.SetFloat("SFXVol", volume);
            PlayerPrefs.Save();

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetSFXVolume(volume);
            }
        }
    }
}

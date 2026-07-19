using UnityEngine;
using EndlessRunner.Core;

namespace EndlessRunner.Audio
{
    /// <summary>
    /// Global sound manager. Handles music loop states, pitch scaling, volume adjustments, and SFX.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Channels")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;

        [Header("Background Tracks")]
        [SerializeField] private AudioClip menuMusic;
        [SerializeField] private AudioClip gameplayMusic;
        [SerializeField] private AudioClip speedBoostMusic;

        [Header("Sound Effects")]
        [SerializeField] private AudioClip coinPickupSFX;
        [SerializeField] private AudioClip gemPickupSFX;
        [SerializeField] private AudioClip jumpSFX;
        [SerializeField] private AudioClip slideSFX;
        [SerializeField] private AudioClip crashSFX;
        [SerializeField] private AudioClip buttonClickSFX;

        private float musicVol = 0.8f;
        private float sfxVol = 0.8f;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                transform.SetParent(null);
                DontDestroyOnLoad(gameObject);
                
                LoadVolumeSettings();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStateChanged += OnGameStateChanged;
            }
            
            PlayMusic(menuMusic);
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStateChanged -= OnGameStateChanged;
            }
        }

        private void LoadVolumeSettings()
        {
            musicVol = PlayerPrefs.GetFloat("MusicVol", 0.8f);
            sfxVol = PlayerPrefs.GetFloat("SFXVol", 0.8f);

            if (musicSource != null) musicSource.volume = musicVol;
            if (sfxSource != null) sfxSource.volume = sfxVol;
        }

        public void SetMusicVolume(float volume)
        {
            musicVol = volume;
            if (musicSource != null) musicSource.volume = musicVol;
        }

        public void SetSFXVolume(float volume)
        {
            sfxVol = volume;
            if (sfxSource != null) sfxSource.volume = sfxVol;
        }

        public void PlayMusic(AudioClip clip)
        {
            if (musicSource == null || clip == null) return;
            if (musicSource.clip == clip && musicSource.isPlaying) return;

            musicSource.clip = clip;
            musicSource.loop = true;
            musicSource.pitch = 1.0f;
            musicSource.Play();
        }

        public void PlaySFX(AudioClip clip)
        {
            if (sfxSource == null || clip == null) return;
            sfxSource.PlayOneShot(clip, sfxVol);
        }

        private void OnGameStateChanged(GameState state)
        {
            switch (state)
            {
                case GameState.MainMenu:
                    PlayMusic(menuMusic);
                    break;
                case GameState.Playing:
                    PlayMusic(gameplayMusic);
                    break;
                case GameState.GameOver:
                    PlayMusic(menuMusic);
                    PlaySFX(crashSFX);
                    break;
            }
        }

        // Sound Effect Trigger Handles
        public void PlayCoinCollected() => PlaySFX(coinPickupSFX);
        public void PlayGemCollected() => PlaySFX(gemPickupSFX);
        public void PlayJump() => PlaySFX(jumpSFX);
        public void PlaySlide() => PlaySFX(slideSFX);
        public void PlayButtonClick() => PlaySFX(buttonClickSFX);

        /// <summary>
        /// Swaps music track and scales pitch to speed up the beat during a sprint boost.
        /// </summary>
        public void ToggleSpeedBoostMusic(bool isBoosting)
        {
            if (musicSource == null) return;
            
            if (isBoosting)
            {
                PlayMusic(speedBoostMusic);
                musicSource.pitch = 1.15f; // Fast tempo
            }
            else
            {
                PlayMusic(gameplayMusic);
                musicSource.pitch = 1.0f;  // Normal speed
            }
        }
    }
}

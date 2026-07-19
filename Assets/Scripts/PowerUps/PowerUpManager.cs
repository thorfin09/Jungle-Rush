using System;
using System.Collections.Generic;
using UnityEngine;
using EndlessRunner.Player;

namespace EndlessRunner.PowerUps
{
    /// <summary>
    /// Manages the activation, ticking down, and deactivation of power-ups.
    /// Spawns associated visual particles and dispatches time-remaining event data to UI.
    /// </summary>
    [RequireComponent(typeof(PlayerController))]
    public class PowerUpManager : MonoBehaviour
    {
        private PlayerController player;

        // Tracks active timers and visual effects
        private readonly Dictionary<PowerUpType, float> activeTimers = new Dictionary<PowerUpType, float>();
        private readonly Dictionary<PowerUpType, float> maxDurations = new Dictionary<PowerUpType, float>();
        private readonly Dictionary<PowerUpType, GameObject> activeVFXs = new Dictionary<PowerUpType, GameObject>();

        // Event hooks for HUD displays
        public event Action<PowerUpType, float> OnPowerUpTimerUpdated; // Emits remaining ratio [0..1]
        public event Action<PowerUpType, bool> OnPowerUpStateChanged;   // Emits true on start, false on end

        public bool IsPowerUpActive(PowerUpType type) => activeTimers.ContainsKey(type);

        private void Awake()
        {
            player = GetComponent<PlayerController>();
        }

        /// <summary>
        /// Activates a power-up and sets up its config duration and visual overlays.
        /// </summary>
        public void ActivatePowerUp(PowerUpConfig config)
        {
            if (config == null) return;

            PowerUpType type = config.type;
            float duration = config.baseDuration;

            // Enable mechanical effects on PlayerController
            SetPlayerPowerUpEffect(type, true);

            // Set up or reset timers
            if (activeTimers.ContainsKey(type))
            {
                activeTimers[type] = duration;
                maxDurations[type] = duration;
            }
            else
            {
                activeTimers.Add(type, duration);
                maxDurations.Add(type, duration);
                OnPowerUpStateChanged?.Invoke(type, true);
            }

            // Handle VFX instantiation
            if (config.activeVFXPrefab != null)
            {
                if (activeVFXs.TryGetValue(type, out GameObject existingVFX))
                {
                    if (existingVFX != null) Destroy(existingVFX);
                }

                GameObject vfx = Instantiate(config.activeVFXPrefab, transform);
                activeVFXs[type] = vfx;
            }
        }

        private void Update()
        {
            // Create a temp list to prevent modification exceptions while iterating
            var activeTypes = new List<PowerUpType>(activeTimers.Keys);

            for (int i = 0; i < activeTypes.Count; i++)
            {
                PowerUpType type = activeTypes[i];
                activeTimers[type] -= Time.deltaTime;
                
                float progress = Mathf.Clamp01(activeTimers[type] / maxDurations[type]);
                OnPowerUpTimerUpdated?.Invoke(type, progress);

                if (activeTimers[type] <= 0f)
                {
                    DeactivatePowerUp(type);
                }
            }
        }

        private void DeactivatePowerUp(PowerUpType type)
        {
            SetPlayerPowerUpEffect(type, false);
            activeTimers.Remove(type);
            maxDurations.Remove(type);

            if (activeVFXs.TryGetValue(type, out GameObject vfx))
            {
                if (vfx != null) Destroy(vfx);
                activeVFXs.Remove(type);
            }

            OnPowerUpStateChanged?.Invoke(type, false);
        }

        /// <summary>
        /// Implements runtime overrides on player properties depending on active power-ups.
        /// </summary>
        private void SetPlayerPowerUpEffect(PowerUpType type, bool active)
        {
            if (player == null) return;

            switch (type)
            {
                case PowerUpType.CoinMagnet:
                    player.IsMagnetActive = active;
                    break;
                case PowerUpType.SpeedBoost:
                    player.IsSpeedBoosting = active;
                    player.IsInvincible = active; // Speed boost naturally grants invincibility
                    break;
                case PowerUpType.Shield:
                    player.IsInvincible = active;
                    break;
                case PowerUpType.DoubleCoins:
                    // Handled inside currency scoring multipliers
                    break;
                case PowerUpType.SlowMotion:
                    Time.timeScale = active ? 0.6f : 1.0f;
                    Time.fixedDeltaTime = 0.02f * Time.timeScale;
                    break;
            }
        }

        private void OnDisable()
        {
            // Safety fallback: reset timescales on close
            Time.timeScale = 1.0f;
            Time.fixedDeltaTime = 0.02f;
        }
    }
}

using UnityEngine;

namespace EndlessRunner.PowerUps
{
    /// <summary>
    /// Configuration data for a specific power-up type.
    /// Configures durations, values, visual icons, and spawned particle effects.
    /// </summary>
    [CreateAssetMenu(fileName = "New PowerUp Config", menuName = "Endless Runner/PowerUp Config")]
    public class PowerUpConfig : ScriptableObject
    {
        public PowerUpType type;
        
        [Tooltip("Active duration in seconds.")]
        public float baseDuration = 8f;

        [Tooltip("Configurable scale value (e.g., multiplier, speed bonus, slow-motion factor).")]
        public float modifierValue = 1f;

        [Tooltip("The HUD icon displaying active status.")]
        public Sprite hudIcon;

        [Tooltip("Prefab instantiated on the player while this power-up is active.")]
        public GameObject activeVFXPrefab;
    }
}

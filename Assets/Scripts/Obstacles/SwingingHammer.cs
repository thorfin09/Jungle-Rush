using UnityEngine;

namespace EndlessRunner.Obstacles
{
    /// <summary>
    /// A dynamic obstacle that swings left and right like a pendulum (perpendicular to player movement).
    /// </summary>
    public class SwingingHammer : Obstacle
    {
        [Tooltip("The speed of the swing cycle.")]
        [SerializeField] private float swingSpeed = 2.5f;

        [Tooltip("The maximum rotation angle in degrees from the center.")]
        [SerializeField] private float maxSwingAngle = 60f;

        [Tooltip("The axis of rotation (e.g. forward Vector3.forward to swing left/right).")]
        [SerializeField] private Vector3 swingAxis = Vector3.forward;

        private float randomOffset;

        private void Start()
        {
            // Give each hammer a random starting phase offset to desynchronize them
            randomOffset = Random.Range(0f, 2f * Mathf.PI);
        }

        private void Update()
        {
            // Compute the pendulum angle using a sine wave
            float angle = Mathf.Sin((Time.time * swingSpeed) + randomOffset) * maxSwingAngle;
            
            // Set rotation relative to its parent (the track tile)
            transform.localRotation = Quaternion.AngleAxis(angle, swingAxis);
        }
    }
}

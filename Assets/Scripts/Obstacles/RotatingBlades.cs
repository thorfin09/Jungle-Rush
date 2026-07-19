using UnityEngine;

namespace EndlessRunner.Obstacles
{
    /// <summary>
    /// A spinning hazard obstacle (like horizontal saw blades or vertical rotating logs).
    /// </summary>
    public class RotatingBlades : Obstacle
    {
        [Tooltip("The rotation speed in degrees per second.")]
        [SerializeField] private float rotationSpeed = 250f;

        [Tooltip("The axis of rotation (e.g., Y-axis for horizontal spinning saw, X-axis for rolling log).")]
        [SerializeField] private Vector3 rotationAxis = Vector3.up;

        private void Update()
        {
            // Rotate the blade constantly in local space
            transform.Rotate(rotationAxis * (rotationSpeed * Time.deltaTime), Space.Self);
        }
    }
}

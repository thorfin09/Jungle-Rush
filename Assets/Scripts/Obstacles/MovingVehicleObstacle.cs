using UnityEngine;

namespace EndlessRunner.Obstacles
{
    /// <summary>
    /// An obstacle that moves dynamically towards the player (like oncoming vehicles).
    /// </summary>
    public class MovingVehicleObstacle : Obstacle
    {
        [Tooltip("The speed at which the vehicle drives towards the player.")]
        [SerializeField] private float moveSpeed = 6f;

        private void Update()
        {
            // Translate the vehicle backward along its local forward axis (towards the oncoming player)
            transform.Translate(Vector3.back * (moveSpeed * Time.deltaTime));
        }
    }
}

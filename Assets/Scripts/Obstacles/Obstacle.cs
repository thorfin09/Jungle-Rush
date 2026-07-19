using UnityEngine;
using EndlessRunner.Player;

namespace EndlessRunner.Obstacles
{
    /// <summary>
    /// Base class for all obstacles.
    /// Deals damage/death to the player when triggered.
    /// </summary>
    public class Obstacle : MonoBehaviour
    {
        [Tooltip("If true, colliding with this obstacle causes instant death. Otherwise, causes a stumble.")]
        [SerializeField] protected bool isLethal = false;

        public bool IsLethal => isLethal;

        protected virtual void OnTriggerEnter(Collider other)
        {
            var player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.HitObstacle(isLethal);
            }
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

namespace EndlessRunner.World
{
    /// <summary>
    /// Represents a single modular track piece in the runner.
    /// Manages its own child elements (coins, obstacles, powerups) for automated cleanup.
    /// </summary>
    public class TrackSegment : MonoBehaviour
    {
        [Tooltip("The Z-axis length of this track segment.")]
        [SerializeField] private float length = 30f;
        
        [Header("Spawn Coordinates")]
        [SerializeField] private Transform[] obstacleSpawnPoints;
        [SerializeField] private Transform[] coinSpawnPoints;
        [SerializeField] private Transform[] powerUpSpawnPoints;

        public float Length => length;
        public Transform[] ObstacleSpawnPoints => obstacleSpawnPoints;
        public Transform[] CoinSpawnPoints => coinSpawnPoints;
        public Transform[] PowerUpSpawnPoints => powerUpSpawnPoints;

        private readonly List<GameObject> activeSpawns = new List<GameObject>();

        /// <summary>
        /// Tracks a spawned object (like a coin or barrier) so it is cleaned up when this tile resets.
        /// </summary>
        public void RegisterSpawnedItem(GameObject item)
        {
            if (item != null)
            {
                activeSpawns.Add(item);
            }
        }

        /// <summary>
        /// Returns this track tile and all of its spawned children to the ObjectPooler.
        /// </summary>
        public void Recycle()
        {
            for (int i = 0; i < activeSpawns.Count; i++)
            {
                GameObject item = activeSpawns[i];
                if (item != null && item.activeSelf)
                {
                    ObjectPooler.Instance.ReturnToPool(item);
                }
            }
            activeSpawns.Clear();
            ObjectPooler.Instance.ReturnToPool(gameObject);
        }
    }
}

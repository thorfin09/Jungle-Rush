using UnityEngine;
using EndlessRunner.Player;
using EndlessRunner.PowerUps;

namespace EndlessRunner.World
{
    /// <summary>
    /// Handles collectible item behavior (coins, gems, power-ups).
    /// Implements visual spin, magnet pull towards the player, collection, and recycling.
    /// </summary>
    public class Collectible : MonoBehaviour
    {
        public enum CollectibleType { Coin, Gem, PowerUp }

        [Header("Collectible Settings")]
        [SerializeField] private CollectibleType type;
        [SerializeField] private PowerUpConfig powerUpConfig; // Assigned only if type is PowerUp
        [SerializeField] private float rotationSpeed = 120f;
        
        [Header("Magnet Pull settings")]
        [SerializeField] private float magnetDistance = 10f;
        [SerializeField] private float magnetSpeed = 16f;

        private PlayerController player;

        private void OnEnable()
        {
            // Dynamically locate the player on enable (since level tiles spawn procedurally)
            if (player == null)
            {
                var playerObj = GameObject.FindWithTag("Player");
                if (playerObj != null)
                {
                    player = playerObj.GetComponent<PlayerController>();
                }
            }
        }

        private void Update()
        {
            // Spin visual model
            transform.Rotate(Vector3.up * (rotationSpeed * Time.deltaTime), Space.Self);

            if (player == null) return;

            // Pull collectible towards character if Magnet powerup is active
            if ((type == CollectibleType.Coin || type == CollectibleType.Gem) && player.IsMagnetActive)
            {
                float distance = Vector3.Distance(transform.position, player.transform.position);
                if (distance <= magnetDistance)
                {
                    Vector3 targetPos = player.transform.position + Vector3.up * 1f; // Aim at character chest
                    transform.position = Vector3.MoveTowards(transform.position, targetPos, magnetSpeed * Time.deltaTime);
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                var pc = other.GetComponent<PlayerController>();
                if (pc != null)
                {
                    Collect(pc);
                }
            }
        }

        private void Collect(PlayerController pc)
        {
            switch (type)
            {
                case CollectibleType.Coin:
                    pc.CollectCoin();
                    break;
                case CollectibleType.Gem:
                    pc.CollectGem();
                    break;
                case CollectibleType.PowerUp:
                    if (powerUpConfig != null)
                    {
                        var pManager = pc.GetComponent<PowerUpManager>();
                        if (pManager != null)
                        {
                            pManager.ActivatePowerUp(powerUpConfig);
                        }
                    }
                    break;
            }

            // Return this item to the ObjectPooler
            ObjectPooler.Instance.ReturnToPool(gameObject);
        }
    }
}

using UnityEngine;
using EndlessRunner.Player;

namespace EndlessRunner.Enemy
{
    /// <summary>
    /// AI controller for the chaser creature that pursues the player.
    /// Closes in on player stumble errors, retreats over time, and executes game-over kills.
    /// </summary>
    public class EnemyChaser : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerController player;
        [SerializeField] private Animator animator;

        [Header("Chaser Spacing")]
        [SerializeField] private float normalChaseOffset = 7.5f;
        [SerializeField] private float closeChaseOffset = 2.2f;
        [SerializeField] private float closeInLerpSpeed = 10f;
        [SerializeField] private float retreatLerpSpeed = 1.8f;

        [Header("Gameplay parameters")]
        [SerializeField] private float retreatDelay = 5.0f; // Seconds of error-free running required to retreat

        private float currentChaseOffset;
        private float retreatTimer;
        private bool isChaserClose;
        private bool isPlayerDead;

        private void Start()
        {
            currentChaseOffset = normalChaseOffset;

            if (player != null)
            {
                player.OnStumble += HandlePlayerStumble;
                player.OnDeath += HandlePlayerDeath;
            }
            
            PlayAnimation("Run");
        }

        private void OnDestroy()
        {
            if (player != null)
            {
                player.OnStumble -= HandlePlayerStumble;
                player.OnDeath -= HandlePlayerDeath;
            }
        }

        private void Update()
        {
            if (player == null) return;

            if (isPlayerDead)
            {
                // Leap forward to attack the fallen player
                Vector3 attackPos = player.transform.position;
                transform.position = Vector3.MoveTowards(transform.position, attackPos, 8f * Time.deltaTime);
                return;
            }

            // Count down retreat buffer
            if (isChaserClose)
            {
                retreatTimer -= Time.deltaTime;
                if (retreatTimer <= 0)
                {
                    isChaserClose = false;
                }
            }

            // Lerp tracking distance depending on error status
            float targetOffset = isChaserClose ? closeChaseOffset : normalChaseOffset;
            float lerpSpeed = isChaserClose ? closeInLerpSpeed : retreatLerpSpeed;
            currentChaseOffset = Mathf.Lerp(currentChaseOffset, targetOffset, lerpSpeed * Time.deltaTime);

            // Follow player path (lagged horizontally for a organic chase path)
            float laggedX = Mathf.Lerp(transform.position.x, player.transform.position.x, 8f * Time.deltaTime);
            float targetZ = player.transform.position.z - currentChaseOffset;
            
            transform.position = new Vector3(laggedX, player.transform.position.y, targetZ);
            transform.rotation = player.transform.rotation;
        }

        private void HandlePlayerStumble()
        {
            isChaserClose = true;
            retreatTimer = retreatDelay;
            PlayAnimation("Growl");
        }

        private void HandlePlayerDeath()
        {
            isPlayerDead = true;
            PlayAnimation("Attack");
        }

        private void PlayAnimation(string triggerName)
        {
            if (animator != null)
            {
                animator.SetTrigger(triggerName);
            }
        }
    }
}

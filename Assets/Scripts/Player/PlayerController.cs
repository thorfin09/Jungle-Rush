using System;
using UnityEngine;
using EndlessRunner.Input;

namespace EndlessRunner.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Lane Settings")]
        [SerializeField] private float laneDistance = 2.5f; // Distance between lanes
        [SerializeField] private float laneSwitchSpeed = 15f; // Speed of lane transition
        
        [Header("Speed Settings")]
        [SerializeField] private float baseForwardSpeed = 10f;
        [SerializeField] private float maxForwardSpeed = 25f;
        [SerializeField] private float speedIncreaseRate = 0.05f; // m/s^2 increase
        [SerializeField] private float speedBoostBonusSpeed = 10f; // Added speed during sprint boost

        [Header("Jump & Slide Settings")]
        [SerializeField] private float jumpForce = 12f;
        [SerializeField] private float gravity = 30f;
        [SerializeField] private float slideDuration = 0.8f;
        [SerializeField] private float stumbleDuration = 1.2f;

        // Public properties accessed by states
        public float Gravity => gravity;
        public float JumpForce => jumpForce;
        public float SlideDuration => slideDuration;
        public float StumbleDuration => stumbleDuration;
        public bool IsGrounded => characterController.isGrounded;
        public bool IsStumbling { get; set; }
        
        // Powerup flags
        public bool IsInvincible { get; set; }
        public bool IsMagnetActive { get; set; }
        public bool IsSpeedBoosting { get; set; }

        // Events for GameManager / UI / Enemy Chaser
        public event Action OnDeath;
        public event Action OnStumble;
        public event Action OnCoinCollected;
        public event Action OnGemCollected;

        private CharacterController characterController;
        private Animator animator;
        private IInputService inputService;
        private PlayerState currentState;
        
        private int targetLane = 1; // 0 = Left, 1 = Middle, 2 = Right
        private float activeForwardSpeed;
        private float verticalVelocity;
        
        private Vector3 originalColliderCenter;
        private float originalColliderHeight;

        public float CurrentForwardSpeed
        {
            get
            {
                if (currentState is DeadState) return 0f;
                float speed = activeForwardSpeed;
                if (IsSpeedBoosting) speed += speedBoostBonusSpeed;
                if (IsStumbling) speed *= 0.5f;
                return speed;
            }
        }

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            animator = GetComponentInChildren<Animator>();
            
            originalColliderHeight = characterController.height;
            originalColliderCenter = characterController.center;
        }

        private void Start()
        {
            activeForwardSpeed = baseForwardSpeed;
            
            // Choose input service based on runtime platform
#if UNITY_EDITOR
            inputService = new KeyboardInputService();
#elif UNITY_ANDROID || UNITY_IOS
            inputService = new MobileInputService();
#else
            inputService = new KeyboardInputService();
#endif

            // Set initial state
            TransitionToState(new RunningState(this));

            // Register input callbacks
            inputService.OnSwipeLeft += () => currentState?.OnSwipeLeft();
            inputService.OnSwipeRight += () => currentState?.OnSwipeRight();
            inputService.OnSwipeUp += () => currentState?.OnSwipeUp();
            inputService.OnSwipeDown += () => currentState?.OnSwipeDown();
        }

        private void Update()
        {
            // Update input reading
            inputService?.Update();

            // Update current state behavior
            currentState?.Update();

            // Increase difficulty speed over time if playing
            if (currentState is not DeadState && !IsSpeedBoosting)
            {
                activeForwardSpeed = Mathf.MoveTowards(activeForwardSpeed, maxForwardSpeed, speedIncreaseRate * Time.deltaTime);
            }

            // Calculate movement displacement
            // Horizontal movement (Lane switching)
            float targetX = (targetLane - 1) * laneDistance;
            float currentX = transform.position.x;
            float newX = Mathf.MoveTowards(currentX, targetX, laneSwitchSpeed * Time.deltaTime);
            float xDisplacement = newX - currentX;

            // Forward and Vertical movement
            float zDisplacement = CurrentForwardSpeed * Time.deltaTime;
            float yDisplacement = verticalVelocity * Time.deltaTime;

            Vector3 moveDelta = new Vector3(xDisplacement, yDisplacement, zDisplacement);
            characterController.Move(moveDelta);
        }

        private void FixedUpdate()
        {
            currentState?.FixedUpdate();
        }

        public void TransitionToState(PlayerState newState)
        {
            currentState?.Exit();
            currentState = newState;
            currentState.Enter();
        }

        public void ChangeLane(int direction)
        {
            targetLane = Mathf.Clamp(targetLane + direction, 0, 2);
        }

        public void SetVerticalVelocity(float velocity)
        {
            verticalVelocity = velocity;
        }

        public void ScaleCollider(bool isSliding)
        {
            if (isSliding)
            {
                characterController.height = originalColliderHeight * 0.5f;
                characterController.center = new Vector3(originalColliderCenter.x, originalColliderCenter.y - (originalColliderHeight * 0.25f), originalColliderCenter.z);
            }
            else
            {
                characterController.height = originalColliderHeight;
                characterController.center = originalColliderCenter;
            }
        }

        public void PlayAnimation(string triggerName)
        {
            if (animator == null) return;
            animator.SetTrigger(triggerName);
        }

        public void TriggerStumbleEvent()
        {
            OnStumble?.Invoke();
        }

        public void TriggerDeathEvent()
        {
            OnDeath?.Invoke();
        }

        public void HitObstacle(bool isLethal)
        {
            if (IsInvincible) return; // Invincible power-up bypasses hits

            if (isLethal)
            {
                TransitionToState(new DeadState(this));
            }
            else
            {
                if (currentState is StumbleState)
                {
                    // Stumbling and got hit again = death
                    TransitionToState(new DeadState(this));
                }
                else if (currentState is RunningState)
                {
                    TransitionToState(new StumbleState(this));
                }
            }
        }

        public void CollectCoin()
        {
            OnCoinCollected?.Invoke();
        }

        public void CollectGem()
        {
            OnGemCollected?.Invoke();
        }
    }
}

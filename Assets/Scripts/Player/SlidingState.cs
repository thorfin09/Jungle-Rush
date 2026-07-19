using UnityEngine;

namespace EndlessRunner.Player
{
    public class SlidingState : PlayerState
    {
        private float slideTimer;
        private float slideDuration;

        public SlidingState(PlayerController controller) : base(controller)
        {
            slideDuration = controller.SlideDuration;
        }

        public override void Enter()
        {
            controller.PlayAnimation("Slide");
            controller.ScaleCollider(true); // Half-height collider for obstacles
            slideTimer = slideDuration;
            controller.SetVerticalVelocity(0f);
        }

        public override void Update()
        {
            slideTimer -= Time.deltaTime;
            if (slideTimer <= 0)
            {
                controller.TransitionToState(new RunningState(controller));
            }
        }

        public override void Exit()
        {
            controller.ScaleCollider(false); // Restore original size
        }

        public override void OnSwipeLeft()
        {
            controller.ChangeLane(-1);
        }

        public override void OnSwipeRight()
        {
            controller.ChangeLane(1);
        }

        public override void OnSwipeUp()
        {
            // Cancel slide and immediately jump
            controller.TransitionToState(new JumpingState(controller));
        }
    }
}

using UnityEngine;

namespace EndlessRunner.Player
{
    public class JumpingState : PlayerState
    {
        private float verticalVelocity;
        private float gravity;
        private float jumpForce;

        public JumpingState(PlayerController controller) : base(controller)
        {
            gravity = controller.Gravity;
            jumpForce = controller.JumpForce;
        }

        public override void Enter()
        {
            controller.PlayAnimation("Jump");
            verticalVelocity = jumpForce;
            controller.SetVerticalVelocity(verticalVelocity);
        }

        public override void Update()
        {
            verticalVelocity -= gravity * Time.deltaTime;
            controller.SetVerticalVelocity(verticalVelocity);

            // Return to running when landing on ground
            if (controller.IsGrounded && verticalVelocity <= 0)
            {
                controller.TransitionToState(new RunningState(controller));
            }
        }

        public override void OnSwipeLeft()
        {
            controller.ChangeLane(-1);
        }

        public override void OnSwipeRight()
        {
            controller.ChangeLane(1);
        }

        public override void OnSwipeDown()
        {
            // Fast fall directly into a slide
            controller.TransitionToState(new SlidingState(controller));
        }
    }
}

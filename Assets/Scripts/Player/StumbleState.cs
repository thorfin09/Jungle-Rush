using UnityEngine;

namespace EndlessRunner.Player
{
    public class StumbleState : PlayerState
    {
        private float stumbleTimer;
        private float stumbleDuration;

        public StumbleState(PlayerController controller) : base(controller)
        {
            stumbleDuration = controller.StumbleDuration;
        }

        public override void Enter()
        {
            controller.PlayAnimation("Stumble");
            controller.IsStumbling = true;
            stumbleTimer = stumbleDuration;
            
            // Notify systems (e.g. Enemy Chaser closes in)
            controller.TriggerStumbleEvent();
        }

        public override void Update()
        {
            stumbleTimer -= Time.deltaTime;
            if (stumbleTimer <= 0)
            {
                controller.TransitionToState(new RunningState(controller));
            }
        }

        public override void Exit()
        {
            controller.IsStumbling = false;
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
            controller.TransitionToState(new JumpingState(controller));
        }

        public override void OnSwipeDown()
        {
            controller.TransitionToState(new SlidingState(controller));
        }
    }
}

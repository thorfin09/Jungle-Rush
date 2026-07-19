using UnityEngine;

namespace EndlessRunner.Player
{
    public class RunningState : PlayerState
    {
        public RunningState(PlayerController controller) : base(controller) {}

        public override void Enter()
        {
            controller.PlayAnimation("Run");
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

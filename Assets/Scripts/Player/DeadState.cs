using UnityEngine;

namespace EndlessRunner.Player
{
    public class DeadState : PlayerState
    {
        public DeadState(PlayerController controller) : base(controller) {}

        public override void Enter()
        {
            controller.PlayAnimation("Die");
            controller.SetVerticalVelocity(0f);
            
            // Dispatch player death event to GameManager
            controller.TriggerDeathEvent();
        }
    }
}

namespace EndlessRunner.Player
{
    public abstract class PlayerState
    {
        protected PlayerController controller;

        protected PlayerState(PlayerController controller)
        {
            this.controller = controller;
        }

        public virtual void Enter() {}
        public virtual void Exit() {}
        public virtual void Update() {}
        public virtual void FixedUpdate() {}
        public virtual void OnSwipeLeft() {}
        public virtual void OnSwipeRight() {}
        public virtual void OnSwipeUp() {}
        public virtual void OnSwipeDown() {}
    }
}

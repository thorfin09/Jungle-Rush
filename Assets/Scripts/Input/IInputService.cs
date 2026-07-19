using System;

namespace EndlessRunner.Input
{
    public interface IInputService
    {
        event Action OnSwipeLeft;
        event Action OnSwipeRight;
        event Action OnSwipeUp;
        event Action OnSwipeDown;

        void Update();
    }
}

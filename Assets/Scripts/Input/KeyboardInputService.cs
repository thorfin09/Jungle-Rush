using System;
using UnityEngine.InputSystem;

namespace EndlessRunner.Input
{
    public class KeyboardInputService : IInputService
    {
        public event Action OnSwipeLeft;
        public event Action OnSwipeRight;
        public event Action OnSwipeUp;
        public event Action OnSwipeDown;

        public void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return;

            if (keyboard.aKey.wasPressedThisFrame || keyboard.leftArrowKey.wasPressedThisFrame)
            {
                OnSwipeLeft?.Invoke();
            }
            else if (keyboard.dKey.wasPressedThisFrame || keyboard.rightArrowKey.wasPressedThisFrame)
            {
                OnSwipeRight?.Invoke();
            }
            else if (keyboard.wKey.wasPressedThisFrame || keyboard.upArrowKey.wasPressedThisFrame || keyboard.spaceKey.wasPressedThisFrame)
            {
                OnSwipeUp?.Invoke();
            }
            else if (keyboard.sKey.wasPressedThisFrame || keyboard.downArrowKey.wasPressedThisFrame)
            {
                OnSwipeDown?.Invoke();
            }
        }
    }
}

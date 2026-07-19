using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace EndlessRunner.Input
{
    public class MobileInputService : IInputService
    {
        public event Action OnSwipeLeft;
        public event Action OnSwipeRight;
        public event Action OnSwipeUp;
        public event Action OnSwipeDown;

        private Vector2 touchStartPosition;
        private float touchStartTime;
        private bool isTrackingTouch;

        private const float MinSwipeDistance = 50f;
        private const float MaxSwipeTime = 0.5f;

        public void Update()
        {
            var touchscreen = Touchscreen.current;
            if (touchscreen == null) return;

            // Start of primary touch press
            if (touchscreen.primaryTouch.press.wasPressedThisFrame)
            {
                touchStartPosition = touchscreen.primaryTouch.position.ReadValue();
                touchStartTime = Time.time;
                isTrackingTouch = true;
            }
            // End of primary touch press
            else if (touchscreen.primaryTouch.press.wasReleasedThisFrame && isTrackingTouch)
            {
                Vector2 touchEndPosition = touchscreen.primaryTouch.position.ReadValue();
                float touchEndTime = Time.time;
                
                float duration = touchEndTime - touchStartTime;
                if (duration <= MaxSwipeTime)
                {
                    Vector2 swipeVector = touchEndPosition - touchStartPosition;
                    if (swipeVector.magnitude >= MinSwipeDistance)
                    {
                        ProcessSwipe(swipeVector);
                    }
                }
                isTrackingTouch = false;
            }
        }

        private void ProcessSwipe(Vector2 swipeVector)
        {
            // Horizontal swipe
            if (Mathf.Abs(swipeVector.x) > Mathf.Abs(swipeVector.y))
            {
                if (swipeVector.x > 0)
                {
                    OnSwipeRight?.Invoke();
                }
                else
                {
                    OnSwipeLeft?.Invoke();
                }
            }
            // Vertical swipe
            else
            {
                if (swipeVector.y > 0)
                {
                    OnSwipeUp?.Invoke();
                }
                else
                {
                    OnSwipeDown?.Invoke();
                }
            }
        }
    }
}

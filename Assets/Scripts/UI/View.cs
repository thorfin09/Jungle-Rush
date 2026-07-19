using UnityEngine;

namespace EndlessRunner.UI
{
    /// <summary>
    /// Base class for modular UI views.
    /// Manages show/hide states using CanvasGroup component parameters.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class View : MonoBehaviour
    {
        protected CanvasGroup canvasGroup;

        protected virtual void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        public virtual void Show()
        {
            gameObject.SetActive(true);
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        public virtual void Hide()
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            gameObject.SetActive(false);
        }
    }
}

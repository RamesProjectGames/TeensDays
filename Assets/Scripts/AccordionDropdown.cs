using UnityEngine;
using UnityEngine.UI;

public class AccordionDropdown : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LayoutElement layoutElement; // The Layout Element on the Class List
    [SerializeField] private CanvasGroup canvasGroup;    // For fading the classes
    
    [Header("Settings")]
    [SerializeField] private float targetHeight = 300f;   // Total height of all class buttons combined
    [SerializeField] private float animationTime = 0.3f;

    private bool isOpen = false;

    void Awake()
    {
        // Start collapsed
        layoutElement.preferredHeight = 0;
        canvasGroup.alpha = 0;
        canvasGroup.blocksRaycasts = false;
    }

    public void ToggleMenu(bool open)
    {
        isOpen = open;

        // Cancel previous tweens to prevent jittering
        LeanTween.cancel(layoutElement.gameObject);
        LeanTween.cancel(canvasGroup.gameObject);

        float startHeight = layoutElement.preferredHeight;
        float endHeight = isOpen ? targetHeight : 0;
        float endAlpha = isOpen ? 1f : 0f;

        var layoutRectTransform = layoutElement.GetComponent<RectTransform>();

        // 1. Animate the height value
        LeanTween.value(layoutElement.gameObject, startHeight, endHeight, animationTime)
            .setEase(LeanTweenType.easeInOutSine)
            .setOnUpdate((float val) => {
                layoutElement.preferredHeight = val;
                layoutRectTransform.sizeDelta = new Vector2(layoutRectTransform.sizeDelta.x, val);
            });

        // 2. Animate the fade
        LeanTween.alphaCanvas(canvasGroup, endAlpha, animationTime);
        canvasGroup.blocksRaycasts = isOpen;
    }
}
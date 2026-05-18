using UnityEngine;
using UnityEngine.UI;

public class AchievementAnnouncementUI : MonoBehaviour
{
    [SerializeField] private Image achievementIcon;
    [SerializeField] private Text titleText;
    [SerializeField] private Text descriptionText;
    [SerializeField] private AudioSource announcementAudio;
    [SerializeField] private float showDuration = 5f;
    [SerializeField] private float animationDuration = 0.7f;
    [SerializeField] private CanvasGroup canvasGroup;

    private RectTransform rectTransform;
    private Vector3 showPosition;
    private Vector3 hidePosition;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        showPosition = rectTransform.anchoredPosition;
        hidePosition = showPosition + Vector3.up * 200f;
        rectTransform.anchoredPosition = hidePosition;
        canvasGroup.alpha = 0;

        AchieveManager.Instance.OnAchievementUnlocked += ShowAchievementAnnouncement;
    }

    void OnDestroy()
    {
        if (AchieveManager.Instance != null)
        {
            AchieveManager.Instance.OnAchievementUnlocked -= ShowAchievementAnnouncement;
        }
    }

    private void ShowAchievementAnnouncement(AchievementData achievementData)
    {
        var scriptable = AchieveManager.Instance.GetAchievementScriptable(achievementData.achievementId);
        if (scriptable == null)
            return;

        // Set content
        if (achievementIcon != null && scriptable.AchievementIcon != null)
            achievementIcon.sprite = scriptable.AchievementIcon;

        if (titleText != null)
            titleText.text = scriptable.Title;

        if (descriptionText != null)
            descriptionText.text = scriptable.Description;

        // Play sound
        if (announcementAudio != null)
            announcementAudio.Play();

        // Animate in
        LeanTween.moveY(rectTransform, showPosition.y, animationDuration)
            .setEase(LeanTweenType.easeOutQuad);
        LeanTween.alphaCanvas(canvasGroup, 1f, animationDuration);

        // Wait and animate out
        LeanTween.delayedCall(showDuration, () =>
        {
            LeanTween.moveY(rectTransform, hidePosition.y, animationDuration)
                .setEase(LeanTweenType.easeInQuad);
            LeanTween.alphaCanvas(canvasGroup, 0f, animationDuration);
        });
    }
}

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class AchievementUIPanel : MonoBehaviour
{
    [SerializeField] private Transform achievementContainer;
    [SerializeField] private AchievementUIItem achievementItemPrefab;
    [SerializeField] private Button closeButton;
    [SerializeField] private Text achievementCountText;
    [SerializeField] private ScrollRect scrollRect;

    private List<AchievementUIItem> instantiatedItems = new List<AchievementUIItem>();

    void Start()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(ClosePanel);

        AchieveManager.Instance.OnAchievementsLoaded += RefreshAchievementList;
        AchieveManager.Instance.OnAchievementUnlocked += OnAchievementUnlocked;
        AchieveManager.Instance.OnAchievementClaimed += OnAchievementClaimedHandler;
    }

    void OnDestroy()
    {
        if (AchieveManager.Instance != null)
        {
            AchieveManager.Instance.OnAchievementsLoaded -= RefreshAchievementList;
            AchieveManager.Instance.OnAchievementUnlocked -= OnAchievementUnlocked;
            AchieveManager.Instance.OnAchievementClaimed -= OnAchievementClaimedHandler;
        }
    }

    private void OnAchievementClaimedHandler(AchievementData achievement)
    {
        RefreshAchievementList();
    }

    private void OnAchievementUnlocked(AchievementData achievement)
    {
        RefreshAchievementList();
        ShowAchievementNotification(achievement);
    }

    public void RefreshAchievementList()
    {
        RefreshAchievementList(AchieveManager.Instance.GetAllAchievements());
    }

    private void RefreshAchievementList(List<AchievementData> achievements)
    {
        // Clear old items
        foreach (var item in instantiatedItems)
        {
            Destroy(item.gameObject);
        }
        instantiatedItems.Clear();

        // Create new items
        foreach (var achievement in achievements)
        {
            var uiItem = Instantiate(achievementItemPrefab, achievementContainer);
            uiItem.Initialize(achievement, AchieveManager.Instance.GetAchievementScriptable(achievement.achievementId));
            instantiatedItems.Add(uiItem);
        }

        // Update achievement count
        var unclaimedCount = AchieveManager.Instance.GetUnclaimedAchievements().Count;
        var totalCount = achievements.Count;
        if (achievementCountText != null)
            achievementCountText.text = $"Achievements: {achievements.Count - unclaimedCount}/{totalCount}";

        // Scroll to top
        if (scrollRect != null)
            Canvas.ForceUpdateCanvases();
    }

    private void ShowAchievementNotification(AchievementData achievement)
    {
        var scriptable = AchieveManager.Instance.GetAchievementScriptable(achievement.achievementId);
        if (scriptable != null)
        {
            Debug.Log($"Achievement Unlocked: {scriptable.Title}!");
        }
    }

    public void OpenPanel()
    {
        gameObject.SetActive(true);
        RefreshAchievementList(AchieveManager.Instance.GetAllAchievements());
    }

    public void ClosePanel()
    {
        gameObject.SetActive(false);
    }
}

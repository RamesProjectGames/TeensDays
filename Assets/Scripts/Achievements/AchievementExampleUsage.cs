using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Example usage showing how to integrate achievements into your game
/// Add this to any GameObject in your scene or call the methods from other scripts
/// </summary>
public class AchievementExampleUsage : MonoBehaviour
{
    // Example: Call this when player completes first level
    public void OnFirstLevelCompleted()
    {
        AchievementHelper.UnlockAchievement("first_level_complete");
        Debug.Log("First level achievement unlocked!");
    }

    // Example: Call this when player earns enough money
    public void OnMoneyMilestone(int totalMoney)
    {
        if (totalMoney >= 50000)
        {
            AchievementHelper.UnlockAchievement("earned_50k");
        }
        if (totalMoney >= 100000)
        {
            AchievementHelper.UnlockAchievement("earned_100k");
        }
    }

    // Example: Call this when player completes a quest
    public void OnQuestComplete(int totalQuestsCompleted)
    {
        if (totalQuestsCompleted == 1)
        {
            AchievementHelper.UnlockAchievement("first_quest");
        }
        if (totalQuestsCompleted == 10)
        {
            AchievementHelper.UnlockAchievement("quest_master");
        }
    }

    // Example: Open achievement panel
    public void OpenAchievementPanel()
    {
        // Find the AchievementUIPanel in your scene
        var panel = FindObjectOfType<AchievementUIPanel>();
        if (panel != null)
        {
            panel.OpenPanel();
        }
    }

    // Example: Listen to achievement events
    void OnEnable()
    {
        if (AchieveManager.Instance != null)
        {
            AchieveManager.Instance.OnAchievementUnlocked += HandleAchievementUnlocked;
            AchieveManager.Instance.OnAchievementClaimed += HandleAchievementClaimed;
        }
    }

    void OnDisable()
    {
        if (AchieveManager.Instance != null)
        {
            AchieveManager.Instance.OnAchievementUnlocked -= HandleAchievementUnlocked;
            AchieveManager.Instance.OnAchievementClaimed -= HandleAchievementClaimed;
        }
    }

    private void HandleAchievementUnlocked(AchievementData achievement)
    {
        Debug.Log($"Achievement Unlocked: {achievement.title}");
        // Add any additional logic here (sounds, particles, etc.)
    }

    private void HandleAchievementClaimed(AchievementData achievement)
    {
        Debug.Log($"Achievement Claimed: {achievement.title}");
        // Add any additional logic here (update UI, etc.)
    }

    // Example: Get achievement statistics
    public void PrintAchievementStats()
    {
        int total = AchievementHelper.GetTotalAchievements();
        int claimed = AchievementHelper.GetClaimedCount();
        int unclaimed = AchievementHelper.GetUnclaimedCount();

        Debug.Log($"Total Achievements: {total}");
        Debug.Log($"Claimed: {claimed}");
        Debug.Log($"Unclaimed: {unclaimed}");
        Debug.Log($"Progress: {claimed}/{total}");
    }

    // Example: Get specific achievement
    public void CheckAchievementStatus(string achievementId)
    {
        AchievementData achievement = AchieveManager.Instance.GetAchievement(achievementId);
        if (achievement != null)
        {
            Debug.Log($"Achievement: {achievement.title}");
            Debug.Log($"Status: {(achievement.isClaimed ? "Claimed" : achievement.obtainedTimestamp > 0 ? "Unlocked" : "Locked")}");
        }
        else
        {
            Debug.LogWarning($"Achievement {achievementId} not found");
        }
    }
}

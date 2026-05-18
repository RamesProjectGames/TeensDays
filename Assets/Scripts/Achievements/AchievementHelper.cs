using UnityEngine;

/// <summary>
/// Helper class to easily unlock and claim achievements
/// Use this in other scripts to trigger achievement events
/// Example: AchievementHelper.UnlockAchievement("first_win");
/// </summary>
public static class AchievementHelper
{
    /// <summary>
    /// Unlocks an achievement by ID
    /// </summary>
    public static void UnlockAchievement(string achievementId)
    {
        if (AchieveManager.Instance != null)
        {
            AchieveManager.Instance.UnlockAchievement(achievementId);
        }
        else
        {
            Debug.LogWarning("AchieveManager instance not found");
        }
    }

    /// <summary>
    /// Claims an achievement reward by ID
    /// </summary>
    public static void ClaimAchievement(string achievementId)
    {
        if (AchieveManager.Instance != null)
        {
            AchieveManager.Instance.ClaimAchievement(achievementId);
        }
        else
        {
            Debug.LogWarning("AchieveManager instance not found");
        }
    }

    /// <summary>
    /// Gets total achievements
    /// </summary>
    public static int GetTotalAchievements()
    {
        if (AchieveManager.Instance != null)
        {
            return AchieveManager.Instance.GetAllAchievements().Count;
        }
        return 0;
    }

    /// <summary>
    /// Gets claimed achievements count
    /// </summary>
    public static int GetClaimedCount()
    {
        if (AchieveManager.Instance != null)
        {
            return AchieveManager.Instance.GetClaimedAchievements().Count;
        }
        return 0;
    }

    /// <summary>
    /// Gets unclaimed achievements count
    /// </summary>
    public static int GetUnclaimedCount()
    {
        if (AchieveManager.Instance != null)
        {
            return AchieveManager.Instance.GetUnclaimedAchievements().Count;
        }
        return 0;
    }
}

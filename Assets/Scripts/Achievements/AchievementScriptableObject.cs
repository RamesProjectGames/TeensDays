using UnityEngine;

[CreateAssetMenu(fileName = "Achievement", menuName = "Game/Achievement")]
public class AchievementScriptableObject : ScriptableObject
{
    [SerializeField] private string achievementId;
    [SerializeField] private string title;
    [SerializeField] private string description;
    [SerializeField] private Sprite achievementIcon;
    [SerializeField] private int rewardAmount = 5000;

    public string AchievementId => achievementId;
    public string Title => title;
    public string Description => description;
    public Sprite AchievementIcon => achievementIcon;
    public int RewardAmount => rewardAmount;

    public AchievementData ToAchievementData(bool isClaimed = false)
    {
        return new AchievementData
        {
            achievementId = achievementId,
            title = title,
            description = description,
            isClaimed = isClaimed,
            obtainedTimestamp = System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        };
    }
}

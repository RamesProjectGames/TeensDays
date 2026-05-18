using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class AchievementUIItem : MonoBehaviour
{
    [SerializeField] private Image achievementIcon;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text rewardText;
    [SerializeField] private Button claimButton;
    [SerializeField] private Image claimedOverlay;
    [SerializeField] private TMP_Text claimedText;

    private string achievementId;

    public void Initialize(AchievementData achievementData, AchievementScriptableObject scriptableObject)
    {
        achievementId = achievementData.achievementId;

        if (scriptableObject != null)
        {
            if (achievementIcon != null && scriptableObject.AchievementIcon != null)
                achievementIcon.sprite = scriptableObject.AchievementIcon;

            if (titleText != null)
                titleText.text = scriptableObject.Title;

            if (descriptionText != null)
                descriptionText.text = scriptableObject.Description;

            if (rewardText != null)
                rewardText.text = $"Reward: {scriptableObject.RewardAmount}";
        }

        // Set up claim button
        bool isClaimed = achievementData.isClaimed;
        bool isUnlocked = achievementData.obtainedTimestamp > 0;

        if (claimButton != null)
        {
            claimButton.interactable = isUnlocked && !isClaimed;
            claimButton.onClick.AddListener(OnClaimButtonPressed);
            
            if(claimButton.TryGetComponent<TMP_Text>(out var claimButtonText))
            {
                if (isClaimed)
                    claimButtonText.text = "Claimed";
                else if (isUnlocked)
                    claimButtonText.text = "Claim";
                else
                    claimButtonText.text = "Locked";
            }
        }

        // Show claimed overlay
        if (claimedOverlay != null)
            claimedOverlay.gameObject.SetActive(isClaimed);

        if (claimedText != null)
            claimedText.gameObject.SetActive(isClaimed);
    }

    private void OnClaimButtonPressed()
    {
        AchieveManager.Instance.ClaimAchievement(achievementId);
    }
}

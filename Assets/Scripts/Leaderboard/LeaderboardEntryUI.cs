using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Networking;
using UnityEngine.UI;

public class LeaderboardEntryUI : MonoBehaviour
{
    public Image backgroundImage; 
    public TMP_Text rankingText;

    public Image profilePicture;

    public TMP_Text timeText;

    public TMP_Text rewardText;


    public void Setup(int ranking, LeaderboardData data)
    {
        backgroundImage.sprite = LeaderboardUIManager.Instance.rankBackgrounds[Mathf.Clamp(ranking - 1, 0, LeaderboardUIManager.Instance.rankBackgrounds.Count - 1)];
        
        rankingText.text = $"#{ranking}";

        timeText.text = FormatTime(data.bestTime);

        rewardText.text = $"{data.rewardAmount} Gold";

        SetPlayerIcon(data.playerIconIndex);
    }

    private void SetPlayerIcon(int index)
    {
        if (ProfilManager.Instance.playerIcons == null || ProfilManager.Instance.playerIcons.Count == 0)
            return;

        index = Mathf.Clamp(index, 0, ProfilManager.Instance.playerIcons.Count - 1);

        profilePicture.sprite = ProfilManager.Instance.playerIcons[index];
    }

    private string FormatTime(long ms)
    {
        long minutes = ms / 60000;
        ms %= 60000;

        long seconds = ms / 1000;
        ms %= 1000;

        return $"Best Time : <b>{minutes:D2}:{seconds:D2}:{ms:D3}</b>";
    }

}

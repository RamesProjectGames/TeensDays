using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class MailItem : MonoBehaviour
{
    public string title, mailId;
    public string body;
    public List<RewardItem> rewardItems;
    public DateTime sentDate, endDate;
    public bool isClaimed, isRead;

    public TextMeshProUGUI titlePreview, datePreview;
    public Image iconPreview;

    public void OpenMail()
    {
        //AudioManager.Singleton.SFXOneShot("Click");
        FirebaseMailManager.Instance.OpenMessage(this);
    }
}

[System.Serializable]
public class RewardItem
{
    public string name;
    public Sprite icon;
    public int amount;
    public RewardType type;
}

public enum RewardType { Coins, Gems}
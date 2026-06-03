using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MailRewardUI : MonoBehaviour
{
    public RewardItem rewardData;
    public Button infoToggle;
    public Image frame, icon;
    public TextMeshProUGUI amount;

    public void CheckDetails()
    {
        //AudioManager.Singleton.SFXOneShot("Click");
        FirebaseMailManager.Instance.CheckRewardDetails(this);
    }
}

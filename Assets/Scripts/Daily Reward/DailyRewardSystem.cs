using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DailyRewardSystem : MonoBehaviour
{
    public PlayerManager PlayerManager;
    public RewardAnimation RewardAnimation;

    public Reward[] rewards;             // Hadiah per hari
    public Button[] rewardButtons;       // Tombol hadiah di UI

    public Sprite claimedSprite;
    public Sprite availableSprite;
    public Sprite lockedSprite;

    private int currentDay;
    private string lastClaimDateKey = "LastClaimDate";
    private string currentDayKey = "CurrentDay";

    void Start()
    {
        LoadProgress();
        UpdateUI();
    }

    void LoadProgress()
    {
        currentDay = GameManager.Instance.playerData.currentDay;

        string lastClaimDate = GameManager.Instance.playerData.lastClaimDate;
        DateTime lastDate;

        if (DateTime.TryParse(lastClaimDate, out lastDate))
        {
            if (lastDate.Date < DateTime.Now.Date)
            {
                // Hari baru, unlock reward berikutnya
                if (currentDay < rewards.Length)
                    currentDay++;
            }
        }
        else
        {
            // Pertama kali main
            currentDay = 1;
        }

        // Save to cloud after loading
        GameManager.Instance.SavePlayerDataToCloud();
    }

    void UpdateUI()
    {
        for (int i = 0; i < rewardButtons.Length; i++)
        {
            Image iconImage = rewardButtons[i].transform.GetChild(0).GetComponent<Image>(); // icon di dalam tombol
            //Text amountText = rewardButtons[i].transform.GetChild(1).GetComponent<Text>(); // jumlah reward

            if (i < rewards.Length)
            {
                iconImage.sprite = rewards[i].icon;
                //amountText.text = rewards[i].isSpecial ? "?" : rewards[i].amount.ToString();
            }

            if (i < currentDay - 1)
            {
                // Sudah diklaim
                rewardButtons[i].interactable = false;
                rewardButtons[i].image.sprite = claimedSprite;
            }
            else if (i == currentDay - 1)
            {
                // Bisa diklaim hari ini
                rewardButtons[i].interactable = true;
                rewardButtons[i].image.sprite = availableSprite;
                int index = i;
                rewardButtons[i].onClick.AddListener(() => ClaimReward(index));
            }
            else
            {
                // Masih terkunci
                rewardButtons[i].interactable = false;
                rewardButtons[i].image.sprite = lockedSprite;
            }
        }
    }

    void ClaimReward(int index)
    {
        Reward reward = rewards[index];
        Debug.Log("Claim reward day: " + (index + 1) + " → " + reward.rewardName);

        if (reward.isSpecial)
        {
            // contoh reward spesial (hadiah random / item unik)
            Debug.Log("Player mendapat hadiah spesial!");
            GameManager.Instance.playerData.specialRewardClaimed = true;
        }
        else
        {
            // contoh reward diamond
            GameManager.Instance.playerData.currDiamond += reward.amount;
        }

        // Update day tracking
        GameManager.Instance.playerData.currentDay = currentDay;
        GameManager.Instance.playerData.lastClaimDate = DateTime.Now.ToString();

        // Save to cloud
        GameManager.Instance.SavePlayerDataToCloud();

        // Update UI
        rewardButtons[index].interactable = false;
        rewardButtons[index].image.sprite = claimedSprite;
        RewardAnimation.PlayAnimation(rewardButtons[index].transform.position);
    }
}

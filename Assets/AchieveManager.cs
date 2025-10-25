using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AchieveManager : MonoBehaviour
{
    public Button[] rewardButtons;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ClaimReward(int _index)
    {
        GameManager.Instance.currMoney += 5000;

        int money = PlayerPrefs.GetInt("Money", 0);
        money += 5000;
        PlayerPrefs.SetInt("Money", money);

        rewardButtons[_index].interactable = false;
    }
}

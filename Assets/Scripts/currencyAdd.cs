using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.InputSystem.LowLevel.InputStateHistory;

public class currencyAdd : MonoBehaviour
{
    public void AddDiamond(int index)
    {
        GameManager.Instance.currDiamond += index;

        int diamond = PlayerPrefs.GetInt("Diamond", 0);
        diamond += index;
        PlayerPrefs.SetInt("Diamond", diamond);
    }

    public void AddMoney(int _index)
    {
        GameManager.Instance.currMoney += _index;

        int money = PlayerPrefs.GetInt("Money", 0);
        money += _index;
        PlayerPrefs.SetInt("Money", money);
    }
}

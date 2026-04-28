using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.InputSystem.LowLevel.InputStateHistory;

public class currencyAdd : MonoBehaviour
{
    public void AddDiamond(int index)
    {
        GameManager.Instance.playerData.currDiamond += index;
        GameManager.Instance.SavePlayerDataToCloud();
    }

    public void AddMoney(int _index)
    {
        GameManager.Instance.playerData.currMoney += _index;
        GameManager.Instance.SavePlayerDataToCloud();
    }
}

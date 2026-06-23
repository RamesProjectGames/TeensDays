using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grocery : Item
{
    void OnEnable()
    {
        OnEnter.AddListener((GameObject go) =>
        {
            GroceriesManager.Instance.ProgressQuest();
        });
    }
    void OnDisable()
    {
        OnEnter.RemoveAllListeners();
    }
}

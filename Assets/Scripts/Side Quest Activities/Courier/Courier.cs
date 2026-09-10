using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Courier : Item
{
    public CourierManager manager;
    public string updateText;
    void OnEnable()
    {
        OnEnter.AddListener((GameObject go) =>
        {
            manager.ProgressQuest(transform);
        });
    }
    void OnDisable()
    {
        OnEnter.RemoveAllListeners();
    }
}

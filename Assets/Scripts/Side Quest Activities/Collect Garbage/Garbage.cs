using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Garbage : Item
{    
    void OnEnable()
    {
        OnEnter.AddListener((GameObject go) =>
        {
            GarbageCollector.Instance.Collect();
            gameObject.SetActive(false);
        });
    }
    void OnDisable()
    {
        OnEnter.RemoveAllListeners();
    }
}

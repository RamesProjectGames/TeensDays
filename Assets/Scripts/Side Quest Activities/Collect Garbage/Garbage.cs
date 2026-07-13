using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Garbage : Item
{    
    void OnEnable()
    {
        OnEnter.AddListener((GameObject go) =>
        {
            var playerInteraction = FindObjectOfType<PlayerInteraction>();
            playerInteraction.SetInteractText("Collect");
            playerInteraction.onAction.AddListener(() =>
            {
                GarbageCollector.Instance.Collect();
                gameObject.SetActive(false);
            });
        });
    }
    void OnDisable()
    {
        var playerInteraction = FindObjectOfType<PlayerInteraction>();
        playerInteraction.SetInteractText("");
        playerInteraction.onAction.RemoveAllListeners();
        OnEnter.RemoveAllListeners();
    }
}

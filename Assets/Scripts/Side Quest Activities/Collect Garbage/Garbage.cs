using UnityEngine;
using UnityEngine.Events;

public class Garbage : Item
{
    private PlayerInteraction currentPlayerInteraction;

    private void OnEnable()
    {
        OnEnter.AddListener(HandleEnter);
        OnExit.AddListener(HandleExit);
        textBubble = "Collect";
        onInteract.AddListener(() =>
        {
            GarbageCollector.Instance.Collect();
            gameObject.SetActive(false);
        });
    }

    private void OnDisable()
    {
        Cleanup();
        OnEnter.RemoveListener(HandleEnter);
        OnExit.RemoveListener(HandleExit);
        onInteract.RemoveAllListeners();
    }

    private void HandleEnter(GameObject go)
    {
        if (go != gameObject)
            return;

        currentPlayerInteraction = FindObjectOfType<PlayerInteraction>();
        if (currentPlayerInteraction == null)
            return;        
    }

    private void HandleExit(GameObject go)
    {
        if (go != gameObject)
            return;

        Cleanup();
    }

    private void Cleanup()
    {        
        currentPlayerInteraction = null;
        textBubble = "";
    }
}

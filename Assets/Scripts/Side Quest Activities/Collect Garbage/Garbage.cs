using UnityEngine;
using UnityEngine.Events;

public class Garbage : Item
{
    private PlayerInteraction currentPlayerInteraction;
    private UnityAction collectAction;

    private void OnEnable()
    {
        OnEnter.AddListener(HandleEnter);
        OnExit.AddListener(HandleExit);
    }

    private void OnDisable()
    {
        Cleanup();
        OnEnter.RemoveListener(HandleEnter);
        OnExit.RemoveListener(HandleExit);
    }

    private void HandleEnter(GameObject go)
    {
        if (go != gameObject)
            return;

        currentPlayerInteraction = FindObjectOfType<PlayerInteraction>();
        if (currentPlayerInteraction == null)
            return;

        currentPlayerInteraction.SetInteractText("Collect");

        if (collectAction != null)
            currentPlayerInteraction.onAction.RemoveListener(collectAction);

        collectAction = () =>
        {
            if (!gameObject.activeInHierarchy)
                return;

            GarbageCollector.Instance.Collect();
            gameObject.SetActive(false);
            Cleanup();
        };

        currentPlayerInteraction.onAction.AddListener(collectAction);
    }

    private void HandleExit(GameObject go)
    {
        if (go != gameObject)
            return;

        Cleanup();
    }

    private void Cleanup()
    {
        if (currentPlayerInteraction != null && collectAction != null)
        {
            currentPlayerInteraction.onAction.RemoveListener(collectAction);
            currentPlayerInteraction.SetInteractText("");
        }

        currentPlayerInteraction = null;
        collectAction = null;
    }
}

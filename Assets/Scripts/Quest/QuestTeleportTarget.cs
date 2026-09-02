using UnityEngine;

public class QuestTeleportTarget : MonoBehaviour
{
    [Header("Quest Teleport")]
    [SerializeField] private string destinationName;
    [SerializeField] private bool teleportOnQuestActivate = true;

    public string DestinationName => destinationName;
    public bool TeleportOnQuestActivate => teleportOnQuestActivate;

    public void TriggerTeleport()
    {
        if (string.IsNullOrEmpty(destinationName))
        {
            Debug.LogWarning($"Quest teleport target '{name}' has no destination configured.");
            return;
        }

        if (TeleportController.Instance == null)
        {
            Debug.LogWarning($"TeleportController not found for quest target '{name}'.");
            return;
        }

        TeleportController.Instance.TeleportTo(destinationName);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportController : MonoBehaviour
{
    [Header("References")]
    public GameObject teleportUI;
    public Transform teleportTarget;
    public Transform player;

    [Header("Settings")]
    public KeyCode teleportKey = KeyCode.E;

    private bool playerInRange = false;
    private bool isTeleporting = false;

    private void Start()
    {
        teleportUI.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            teleportUI.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            teleportUI.SetActive(false);
        }
    }

    private void Update()
    {
        if (playerInRange && !isTeleporting && Input.GetKeyDown(teleportKey))
        {
            StartCoroutine(TeleportProcess());
        }
    }

    IEnumerator TeleportProcess()
    {
        isTeleporting = true;
        teleportUI.SetActive(false);

        // Fade Out
        yield return fadeControllerTeleport.Instance.FadeOut();

        // Optional: matikan movement
        var controller = player.GetComponent<fadeControllerTeleport>();
        if (controller != null) controller.enabled = false;

        // Teleport
        player.position = teleportTarget.position;

        // Fade In
        yield return fadeControllerTeleport.Instance.FadeIn();

        if (controller != null) controller.enabled = true;

        isTeleporting = false;
    }
}

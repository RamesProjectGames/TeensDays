using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TeleportLocation
{
    public string locationName;
    public Transform targetPoint;
}

public class TeleportController : MonoBehaviour
{

    public static TeleportController Instance;

    [Header("References")]
    public Transform player;
    public GameObject teleportUI;
    public GameObject teleportCanva;

    [Header("Teleport Locations")]
    public TeleportLocation[] locations;

    private bool isTeleporting;

    private void Awake()
    {
        Instance = this;
    }

    public void TeleportTo(string locationName)
    {
        if (isTeleporting) return;

        foreach (var loc in locations)
        {
            if (loc.locationName == locationName)
            {
                StartCoroutine(TeleportProcess(loc.targetPoint));
                return;
            }
        }

        Debug.LogWarning("Lokasi tidak ditemukan: " + locationName);
    }

    IEnumerator TeleportProcess(Transform target)
    {
        isTeleporting = true;
        teleportUI.SetActive(false);

        yield return fadeControllerTeleport.Instance.FadeOut();

        // Disable movement (opsional)
        //var movement = player.GetComponent<PlayerController>();
        //if (movement != null) movement.enabled = false;

        player.position = target.position;

        yield return fadeControllerTeleport.Instance.FadeIn();

        //if (movement != null) movement.enabled = true;

        isTeleporting = false;
    }
    //[Header("References")]
    //public GameObject teleportUI;
    //public Transform teleportTarget;
    //public Transform player;

    //[Header("Settings")]
    //public KeyCode teleportKey = KeyCode.E;

    private bool playerInRange = false;
    //private bool isTeleporting = false;

    //private void Start()
    //{
    //    teleportUI.SetActive(false);
    //}

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
            teleportCanva.SetActive(false);
        }
    }

    //private void Update()
    //{
    //    if (playerInRange && !isTeleporting && Input.GetKeyDown(teleportKey))
    //    {
    //        StartCoroutine(TeleportProcess());
    //    }
    //}

    //IEnumerator TeleportProcess()
    //{
    //    isTeleporting = true;
    //    teleportUI.SetActive(false);

    //    // Fade Out
    //    yield return fadeControllerTeleport.Instance.FadeOut();

    //    // Optional: matikan movement
    //    var controller = player.GetComponent<fadeControllerTeleport>();
    //    if (controller != null) controller.enabled = false;

    //    // Teleport
    //    player.position = teleportTarget.position;

    //    // Fade In
    //    yield return fadeControllerTeleport.Instance.FadeIn();

    //    if (controller != null) controller.enabled = true;

    //    isTeleporting = false;
    //}

    //public void TeleportPasar()
    //{

    //}
}

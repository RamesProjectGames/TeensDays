using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

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
    public NavMeshAgent navMeshAgent;
    public GameObject teleportUI;
    public GameObject teleportCanva;

    [Header("Teleport Settings")]
    public float teleportOffsetFromTarget = 2.5f;

    [Header("Teleport Locations")]
    public TeleportLocation[] locations;

    private bool isTeleporting;

    private void Awake()
    {
        Instance = this;
        ResolvePlayerReferences();
    }

    private void Start()
    {
        ResolvePlayerReferences();
    }

    private void ResolvePlayerReferences()
    {
        if (player == null)
        {
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }

        if (navMeshAgent == null && player != null)
        {
            navMeshAgent = player.GetComponentInChildren<NavMeshAgent>();
        }
    }

    public void TeleportTo(Transform target)
    {
        if (target == null)
        {
            Debug.LogWarning("Target teleport tidak valid.");
            return;
        }

        if (isTeleporting) return;

        ResolvePlayerReferences();
        if (player == null)
        {
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj == null)
            {
                Debug.LogWarning("Player tidak ditemukan untuk teleport.");
                return;
            }
            player = playerObj.transform;
            navMeshAgent = player.GetComponentInChildren<NavMeshAgent>();
        }

        StartCoroutine(TeleportProcess(target));
    }

    public void TeleportTo(string locationName)
    {
        if (isTeleporting) return;
        if (player == null)
        {
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj == null)
            {
                Debug.LogWarning("Player tidak ditemukan untuk teleport.");
                return;
            }
            player = playerObj.transform;
        }

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

    private Vector3 GetTeleportPositionInFrontOfTarget(Transform target)
    {
        Vector3 forward = target.forward;
        if (forward.sqrMagnitude < 0.01f)
        {
            forward = Vector3.forward;
        }

        return target.position + forward.normalized * teleportOffsetFromTarget;
    }

    IEnumerator TeleportProcess(Transform target)
    {
        if (target == null || player == null)
        {
            Debug.LogWarning("Teleport target atau player belum siap.");
            isTeleporting = false;
            yield break;
        }

        ResolvePlayerReferences();

        isTeleporting = true;
        if (teleportUI != null) teleportUI.SetActive(false);

        if (fadeControllerTeleport.Instance != null)
        {
            yield return fadeControllerTeleport.Instance.FadeOut();
        }

        Vector3 destination = GetTeleportPositionInFrontOfTarget(target);
        player.position = destination;

        if (navMeshAgent != null)
        {
            navMeshAgent.Warp(destination);
        }

        if (player != null)
        {
            Vector3 lookDirection = target.position - player.position;
            if (lookDirection.sqrMagnitude > 0.01f)
            {
                player.rotation = Quaternion.LookRotation(lookDirection.normalized, Vector3.up);
            }
        }

        if (navMeshAgent != null)
        {
            navMeshAgent.ResetPath();
            navMeshAgent.velocity = Vector3.zero;
        }

        if (fadeControllerTeleport.Instance != null)
        {
            yield return fadeControllerTeleport.Instance.FadeIn();
        }

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
            if (teleportUI != null) teleportUI.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (teleportUI != null) teleportUI.SetActive(false);
            if (teleportCanva != null) teleportCanva.SetActive(false);
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

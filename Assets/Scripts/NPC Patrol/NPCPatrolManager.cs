using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.AI;

public class NPCPatrolManager : MonoBehaviour
{
    [Header("Patrol Settings (Global)")]
    public float patrolRadius = 15f;
    public float waitTime = 2f;
    public float minMoveDistance = 2f;

    [Header("NPC Groups by Tag")]
    public string[] npcTags = { "Murid SD", "Murid SMP", "Guru", "Satpam" };

    [Header("Waypoint Groups (Assign di Inspector)")]
    public Transform[] sdWaypoints;        // 5 titik untuk Murid SD
    public Transform[] smpWaypoints;       // 5 titik untuk Murid SMP
    public Transform[] smaWaypoints;       // 5 titik untuk Murid SMA
    public Transform[] karyawanWaypoints;  // 7 titik untuk Guru & Satpam

    private List<NPCDataPatrol> npcListPatrol = new List<NPCDataPatrol>();

    private void Start()
    {
        RegisterAllNPCs();
        StartCoroutine(HandlePatrols());
        //StartCoroutine(RefreshActiveNPCs());
    }

    private void RegisterAllNPCs()
    {
        foreach (string tag in npcTags)
        {
            GameObject[] foundNPCs = GameObject.FindGameObjectsWithTag(tag);
            foreach (GameObject npc in foundNPCs)
            {
                RegisterNPC(npc, tag);
            }
        }
    }

    // 🧩 Daftarkan 1 NPC (bisa dipanggil ulang kalau aktif lagi)
    private void RegisterNPC(GameObject npc, string tag)
    {
        if (npcListPatrol.Exists(n => n.npc == npc))
            return;

        NavMeshAgent agent = npc.GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            Debug.LogWarning($"NPC {npc.name} tidak punya NavMeshAgent.");
            return;
        }

        if (!agent.isOnNavMesh)
        {
            NavMeshHit hit;
            if (NavMesh.SamplePosition(npc.transform.position, out hit, 3f, NavMesh.AllAreas))
            {
                npc.transform.position = hit.position;
            }
            else
            {
                Debug.LogWarning($"NPC {npc.name} tidak berada di NavMesh!");
                return;
            }
        }

        Transform[] waypoints = GetWaypointsByTag(tag);
        if (waypoints == null || waypoints.Length == 0)
        {
            Debug.LogWarning($"NPC {npc.name} (tag {tag}) tidak punya waypoint.");
            return;
        }

        NPCDataPatrol data = new NPCDataPatrol(npc, agent, waypoints);
        npcListPatrol.Add(data);

        MoveToNextWaypoint(data);
    }

    //private IEnumerator RefreshActiveNPCs()
    //{
    //    while (true)
    //    {
    //        foreach (string tag in npcTags)
    //        {
    //            GameObject[] foundNPCs = GameObject.FindGameObjectsWithTag(tag);
    //            foreach (GameObject npc in foundNPCs)
    //            {
    //                // Skip jika NPC sudah ada di list
    //                if (npcListPatrol.Exists(n => n.npc == npc)) continue;

    //                // Tambahkan hanya jika memiliki NavMeshAgent dan aktif
    //                NavMeshAgent agent = npc.GetComponent<NavMeshAgent>();
    //                if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
    //                {
    //                    int mask = GetAreaMaskByTag(tag);
    //                    var data = new NPCDataPatrol(npc, agent, mask);
    //                    data.startPos = npc.transform.position;
    //                    data.direction = npc.transform.forward;
    //                    npcListPatrol.Add(new NPCDataPatrol(npc, agent, mask));

    //                    // MoveToRandomPoint(agent, mask, npc.transform.position);

    //                    MoveInLine(data);

    //                    Debug.Log($"NPC {npc.name} aktif kembali dan ditambahkan ke patrol list");
    //                }
    //            }
    //        }

    //        yield return new WaitForSeconds(2f); // cek tiap 2 detik
    //    }
    //}

    private IEnumerator HandlePatrols()
    {
        while (true)
        {
            npcListPatrol.RemoveAll(npc => npc.npc == null);

            foreach (NPCDataPatrol npcData in npcListPatrol)
            {
                if (!npcData.npc.activeInHierarchy || npcData.agent == null)
                    continue;

                if (!npcData.agent.isOnNavMesh)
                {
                    // kalau agent belum balik ke NavMesh, reposition
                    NavMeshHit hit;
                    if (NavMesh.SamplePosition(npcData.npc.transform.position, out hit, 2f, NavMesh.AllAreas))
                    {
                        npcData.npc.transform.position = hit.position;
                    }
                    continue;
                }

                if (!npcData.agent.pathPending && npcData.agent.remainingDistance <= npcData.agent.stoppingDistance)
                {
                    StartCoroutine(WaitAndMove(npcData));
                }
            }
            yield return null;
        }
    }

    //private IEnumerator StraightWaitAndMove(NPCDataPatrol npcData)
    //{
    //    npcData.isWaiting = true;
    //    yield return new WaitForSeconds(waitTime);
    //    // Balik arah
    //    npcData.direction = -npcData.direction;
    //    MoveInLine(npcData);
    //    npcData.isWaiting = false;
    //}

    //private void MoveInLine(NPCDataPatrol npcData)
    //{
    //    Vector3 targetPos = npcData.startPos + npcData.direction * patrolRadius;
    //    NavMeshHit hit;
    //    if (NavMesh.SamplePosition(targetPos, out hit, 3f, npcData.areaMask))
    //    {
    //        npcData.agent.SetDestination(hit.position);
    //    }
    //    else
    //    {
    //        // Jika tidak ada titik valid, coba sedikit di arah sebaliknya
    //        npcData.direction = -npcData.direction;
    //        targetPos = npcData.startPos + npcData.direction * patrolRadius;
    //        if (NavMesh.SamplePosition(targetPos, out hit, 3f, npcData.areaMask))
    //        {
    //            npcData.agent.SetDestination(hit.position);
    //        }
    //    }
    //}

    #region NPC Move Random Point
    private IEnumerator WaitAndMove(NPCDataPatrol npcData)
    {
        npcData.isWaiting = true;
        yield return new WaitForSeconds(waitTime);
        MoveToNextWaypoint(npcData);
        //MoveToRandomPoint(npcData.agent, npcData.areaMask, npcData.npc.transform.position);
        npcData.isWaiting = false;
    }

    private void MoveToNextWaypoint(NPCDataPatrol npc)
    {
        if (npc.waypoints == null || npc.waypoints.Length == 0) return;

        npc.agent.SetDestination(npc.waypoints[npc.currentWaypointIndex].position);

        // Cek arah gerak
        if (npc.movingForward)
        {
            npc.currentWaypointIndex++;
            if (npc.currentWaypointIndex >= npc.waypoints.Length)
            {
                npc.currentWaypointIndex = npc.waypoints.Length - 2;
                npc.movingForward = false;
            }
        }
        else
        {
            npc.currentWaypointIndex--;
            if (npc.currentWaypointIndex < 0)
            {
                npc.currentWaypointIndex = 1;
                npc.movingForward = true;
            }
        }
    }

    //private void MoveToRandomPoint(NavMeshAgent agent, int areaMask, Vector3 origin)
    //{
    //    Vector3 randomDirection = Random.insideUnitSphere * patrolRadius;
    //    randomDirection += origin;

    //    NavMeshHit hit;
    //    if (NavMesh.SamplePosition(randomDirection, out hit, patrolRadius, areaMask))
    //    {
    //        if (Vector3.Distance(origin, hit.position) >= minMoveDistance)
    //        {
    //            agent.SetDestination(hit.position);
    //        }
    //        else
    //        {
    //            MoveToRandomPoint(agent, areaMask, origin); // cari titik lain
    //        }
    //    }
    //}
    #endregion

    private Transform[] GetWaypointsByTag(string tag)
    {
        switch (tag)
        {
            case "Murid SD": return sdWaypoints;
            case "Murid SMP": return smpWaypoints;
            case "Murid SMA": return smaWaypoints;
            case "Guru":
            case "Satpam": return karyawanWaypoints;
            default: return null;
        }
    }

    private int GetAreaMaskByTag(string npcTag)
    {
        switch (npcTag)
        {
            case "Murid SD":
                return 1 << NavMesh.GetAreaFromName("SD"); // hanya area SD
            case "Murid SMP":
                return 1 << NavMesh.GetAreaFromName("SMP"); // hanya area SMP
            case "Guru":
            case "Satpam":
                return 1 << NavMesh.GetAreaFromName("KaryawanSekolah"); // seluruh area sekolah
            default:
                return NavMesh.AllAreas;
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Biar kelihatan garis waypoint-nya di Scene
        Gizmos.color = Color.green;

        DrawWaypointLine(sdWaypoints);
        DrawWaypointLine(smpWaypoints);
        DrawWaypointLine(smaWaypoints);

        Gizmos.color = Color.yellow;
        DrawWaypointLine(karyawanWaypoints);
    }

    private void DrawWaypointLine(Transform[] points)
    {
        if (points == null || points.Length < 2) return;
        for (int i = 0; i < points.Length - 1; i++)
        {
            Gizmos.DrawLine(points[i].position, points[i + 1].position);
            Gizmos.DrawSphere(points[i].position, 0.3f);
        }
    }
}

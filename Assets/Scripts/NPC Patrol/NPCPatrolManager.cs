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

    private List<NPCDataPatrol> npcListPatrol = new List<NPCDataPatrol>();

    private void Start()
    {
        // Cari semua NPC berdasarkan tag yang terdaftar
        foreach (string tag in npcTags)
        {
            GameObject[] foundNPCs = GameObject.FindGameObjectsWithTag(tag);
            foreach (GameObject npc in foundNPCs)
            {
                NavMeshAgent agent = npc.GetComponent<NavMeshAgent>();
                if (agent == null)
                {
                    Debug.LogWarning($"NPC {npc.name} tidak memiliki NavMeshAgent, dilewati.");
                    continue;
                }

                int mask = GetAreaMaskByTag(tag);
                npcListPatrol.Add(new NPCDataPatrol(npc, agent, mask));
                MoveToRandomPoint(agent, mask, npc.transform.position);
            }
        }

        StartCoroutine(HandlePatrols());
    }

    private IEnumerator HandlePatrols()
    {
        while (true)
        {
            foreach (NPCDataPatrol npcData in npcListPatrol)
            {
                if (npcData.agent == null || npcData.isWaiting) continue;

                // Jika NPC sudah sampai di tujuan
                if (!npcData.agent.pathPending && npcData.agent.remainingDistance <= npcData.agent.stoppingDistance)
                {
                    StartCoroutine(WaitAndMove(npcData));
                }
            }

            yield return null; // per frame cek
        }
    }

    private IEnumerator WaitAndMove(NPCDataPatrol npcData)
    {
        npcData.isWaiting = true;
        yield return new WaitForSeconds(waitTime);
        MoveToRandomPoint(npcData.agent, npcData.areaMask, npcData.npc.transform.position);
        npcData.isWaiting = false;
    }

    private void MoveToRandomPoint(NavMeshAgent agent, int areaMask, Vector3 origin)
    {
        Vector3 randomDirection = Random.insideUnitSphere * patrolRadius;
        randomDirection += origin;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(randomDirection, out hit, patrolRadius, areaMask))
        {
            if (Vector3.Distance(origin, hit.position) >= minMoveDistance)
            {
                agent.SetDestination(hit.position);
            }
            else
            {
                MoveToRandomPoint(agent, areaMask, origin); // cari titik lain
            }
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
        Gizmos.color = Color.yellow;
        foreach (NPCDataPatrol npc in npcListPatrol)
        {
            if (npc.npc != null)
                Gizmos.DrawWireSphere(npc.npc.transform.position, patrolRadius);
        }
    }
}

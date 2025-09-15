using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPC_AI : MonoBehaviour
{
    [Header("Waypoints")]
    public List<Transform> wayPoints;

    private NavMeshAgent agent;
    private int currentWaypointIndex = 0;

    [Header("Settings")]
    public float waypointThreshold = 1f; // Jarak minimum untuk ganti waypoint

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            Debug.LogError("NavMeshAgent tidak ditemukan di GameObject ini!");
            enabled = false;
            return;
        }

        if (wayPoints.Count > 0)
        {
            agent.SetDestination(wayPoints[currentWaypointIndex].position);
        }
    }

    void Update()
    {
        Patrol();
    }

    private void Patrol()
    {
        if (wayPoints.Count == 0) return;

        // Cek apakah agent sudah dekat dengan waypoint
        if (!agent.pathPending && agent.remainingDistance <= waypointThreshold)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % wayPoints.Count;
            agent.SetDestination(wayPoints[currentWaypointIndex].position);
        }
    }
}

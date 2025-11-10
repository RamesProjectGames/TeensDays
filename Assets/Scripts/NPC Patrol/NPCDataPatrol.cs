using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPCDataPatrol
{
    public GameObject npc;
    public NavMeshAgent agent;
    public Transform[] waypoints;
    public int currentWaypointIndex;
    public bool movingForward;
    //public int areaMask;
    public bool isWaiting;
    //public Vector3 startPos;
    //public Vector3 direction;

    public NPCDataPatrol(GameObject npc, NavMeshAgent agent, Transform[] waypoints)
    {
        this.npc = npc;
        this.agent = agent;
        this.waypoints = waypoints;
        currentWaypointIndex = 0;
        movingForward = true;
        //this.areaMask = areaMask;
        isWaiting = false;
    }
}

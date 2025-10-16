using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPCDataPatrol
{
    public GameObject npc;
    public NavMeshAgent agent;
    public int areaMask;
    public bool isWaiting;

    public NPCDataPatrol(GameObject npc, NavMeshAgent agent, int areaMask)
    {
        this.npc = npc;
        this.agent = agent;
        this.areaMask = areaMask;
        isWaiting = false;
    }
}

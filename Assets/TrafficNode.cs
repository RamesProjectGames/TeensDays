using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrafficNode : MonoBehaviour
{
    [Header("Connections")]
    public List<TrafficNode> connections = new List<TrafficNode>();

    [Header("Node Settings")]
    public bool isSpawnPoint;
    public bool isDestination;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;

        Gizmos.DrawSphere(transform.position, 0.2f);

        if (connections == null)
            return;

        Gizmos.color = Color.cyan;

        foreach (TrafficNode node in connections)
        {
            if (node == null)
                continue;

            Gizmos.DrawLine(
                transform.position,
                node.transform.position
            );
        }
    }
}

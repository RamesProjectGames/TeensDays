using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(LineRenderer))]
public class QuestPathManager : MonoBehaviour
{
    public Transform player;         // Referensi ke posisi player
    public Transform questTarget;    // Tujuan quest (NPC / lokasi)
    public NavMeshAgent agent;       // Agent hanya dipakai untuk akses NavMesh data

    private LineRenderer line;

    float lastPathLength = 0f;
    float updateThreshold = 0.5f; // meter

    void Start()
    {
        line = GetComponent<LineRenderer>();
        line.positionCount = 0;
        line.widthMultiplier = 0.2f;
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.startColor = Color.yellow;
        line.endColor = Color.yellow;
    }

    void Update()
    {
        if (questTarget == null || player == null)
        {
            line.positionCount = 0;
            return;
        }

        DrawPathToTarget();
    }

    void DrawPathToTarget()
    {
        NavMeshPath path = new NavMeshPath();

        float totalLength = 0f;
        for (int i = 0; i < path.corners.Length - 1; i++)
        {
            totalLength += Vector3.Distance(path.corners[i], path.corners[i + 1]);
        }

        // Update texture hanya jika beda cukup jauh
        if (Mathf.Abs(totalLength - lastPathLength) > updateThreshold)
        {
            float arrowSize = 1.5f;
            line.material.mainTextureScale = new Vector2(
                totalLength / arrowSize,
                1
            );

            lastPathLength = totalLength;
        }

        //
        //if (NavMesh.CalculatePath(player.position, questTarget.position, NavMesh.AllAreas, path))
        //{
        //    line.positionCount = path.corners.Length;
        //    line.SetPositions(path.corners);
        //}
    }

    // Panggil ini ketika quest aktif atau selesai
    public void SetQuestTarget(Transform target)
    {
        questTarget = target;
    }

    public void ClearPath()
    {
        questTarget = null;
        line.positionCount = 0;
    }
}

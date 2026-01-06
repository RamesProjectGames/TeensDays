using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(LineRenderer))]
public class QuestPathManager : MonoBehaviour
{
    public Transform player;         // Referensi ke posisi player
    public Transform questTarget;    // Tujuan quest (NPC / lokasi)
    public NavMeshAgent agent;       // Agent hanya dipakai untuk akses NavMesh data

    private LineRenderer line;

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
        if (NavMesh.CalculatePath(player.position, questTarget.position, NavMesh.AllAreas, path))
        {
            line.positionCount = path.corners.Length;
            line.SetPositions(path.corners);
        }
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

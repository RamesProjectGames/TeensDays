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

    bool isFirstDraw = true;
    float lastPathLength = 0f;
    float updateThreshold = 0.5f; // meter

    public static QuestPathManager Instance;

    Vector3 lastPlayerPos;
    float recalcDistance = 1.0f; // meter
    List<Vector3> cachedPath = new List<Vector3>();

    int lockedSize = -1;
    int maxCornerChange = 1; // toleransi
    Vector3[] smoothPositions;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Start()
    {
        line = GetComponent<LineRenderer>();

        lastPlayerPos = player.position;

        line.positionCount = 0;
        line.widthMultiplier = 0.2f;

        // JANGAN override material
        // line.material = new Material(Shader.Find("Sprites/Default"));

        line.startColor = Color.yellow;
        line.endColor = Color.yellow;

        // 🔥 DRAW PERTAMA KALI
        if (questTarget != null)
            CalculateAndCachePath();
    }

    void Update()
    {
        if (player == null || questTarget == null)
            return;

        if (Vector3.Distance(player.position, lastPlayerPos) >= recalcDistance)
        {
            CalculateAndCachePath();
            lastPlayerPos = player.position;
        }
    }

    //void LateUpdate()
    //{
    //    if (line.positionCount == 0) return;

    //    if (smoothPositions == null || smoothPositions.Length != line.positionCount)
    //        smoothPositions = new Vector3[line.positionCount];

    //    for (int i = 0; i < line.positionCount; i++)
    //    {
    //        Vector3 target = line.GetPosition(i);

    //        // 🔥 TITIK PERTAMA JANGAN DISMOOTH
    //        if (i == 0)
    //        {
    //            smoothPositions[i] = target;
    //            line.SetPosition(i, target);
    //            continue;
    //        }

    //        if (smoothPositions[i] == Vector3.zero)
    //            smoothPositions[i] = target;

    //        smoothPositions[i] = Vector3.Lerp(
    //            smoothPositions[i],
    //            target,
    //            Time.deltaTime * 10f
    //        );

    //        line.SetPosition(i, smoothPositions[i]);
    //    }
    //}

    //void DrawPathToTarget()
    //{
    //    NavMeshPath path = new NavMeshPath();

    //    if (!NavMesh.CalculatePath(player.position, questTarget.position, NavMesh.AllAreas, path))
    //        return;

    //    // 1️⃣ Selalu set posisi line
    //    Vector3[] points = path.corners;

    //    float yOffset = 0.05f;
    //    for (int i = 0; i < points.Length; i++)
    //        points[i] += Vector3.up * yOffset;

    //    line.positionCount = points.Length;
    //    line.SetPositions(points);

    //    // 2️⃣ Hitung panjang path
    //    float totalLength = 0f;
    //    for (int i = 0; i < points.Length - 1; i++)
    //        totalLength += Vector3.Distance(points[i], points[i + 1]);

    //    // 3️⃣ Update texture scale
    //    if (isFirstDraw || Mathf.Abs(totalLength - lastPathLength) > updateThreshold)
    //    {
    //        float arrowSize = 1.5f;
    //        line.material.mainTextureScale = new Vector2(
    //            totalLength / arrowSize,
    //            1
    //        );

    //        lastPathLength = totalLength;
    //        isFirstDraw = false;
    //    }

    //    //
    //    //if (NavMesh.CalculatePath(player.position, questTarget.position, NavMesh.AllAreas, path))
    //    //{
    //    //    line.positionCount = path.corners.Length;
    //    //    line.SetPositions(path.corners);
    //    //}
    //}

    void DrawStablePath()
    {
        int currentSize = cachedPath.Count;

        if (lockedSize < 0)
            lockedSize = currentSize;

        // 🔒 Lock size (toleransi perubahan kecil)
        if (currentSize > lockedSize)
        {
            lockedSize = currentSize;
        }
        // Only shrink if difference is significant
        else if (lockedSize - currentSize > maxCornerChange)
        {
            lockedSize = currentSize;
        }

        line.positionCount = lockedSize;

        for (int i = 0; i < lockedSize; i++)
        {
            int index = Mathf.Min(i, cachedPath.Count - 1);
            Vector3 pos = cachedPath[index];

            // 🔥 Anchor ke player (hilangkan gap)
            if (i == 0)
            {
                pos = player.position + Vector3.up * 0.05f;
            }

            line.SetPosition(i, pos);
        }
    }

    void CalculateAndCachePath()
    {
        NavMeshPath navPath = new NavMeshPath();

        if (!NavMesh.CalculatePath(player.position, questTarget.position, NavMesh.AllAreas, navPath))
            return;

        if (navPath.status != NavMeshPathStatus.PathComplete)
        {
            // Optional: still draw partial path if you want
            return;
        }

        if(HasPathChanged(navPath))
        {
            cachedPath.Clear();            
        }

        for (int i = 0; i < navPath.corners.Length; i++)
        {
            cachedPath.Add(navPath.corners[i] + Vector3.up * 0.05f);
        }

        DrawStablePath();
    }

    // Panggil ini ketika quest aktif atau selesai
    public void SetQuestTarget(Transform target)
    {
        questTarget = target;

        questTarget = target;
        lockedSize = -1;
        cachedPath.Clear();
        CalculateAndCachePath();
    }
    bool HasPathChanged(NavMeshPath newPath)
    {
        if (newPath.corners.Length != cachedPath.Count)
            return true;

        for (int i = 0; i < newPath.corners.Length; i++)
        {
            if (Vector3.Distance(newPath.corners[i], cachedPath[i]) > 0.1f)
                return true;
        }

        return false;
    }
    public void ClearPath()
    {
        questTarget = null;
        if (cachedPath.Count < 2)
        {
            line.positionCount = 0;
            return;
        }
    }
}

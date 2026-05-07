using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(LineRenderer))]
public class QuestPathManager : MonoBehaviour
{    
    public static QuestPathManager Instance;
    [Header("References")]
    public Transform player;
    public Transform questTarget;
    public NavMeshAgent agent;

    [Header("Line Renderer")]
    public LineRenderer line;

    [Header("Path Settings")]
    public float refreshRate = 0.15f;
    public float recalcDistance = 0.3f;
    public float lineHeight = 0.05f;

    [Header("Curve Smoothness")]
    [Range(2, 12)]
    public int splineResolution = 6;

    [Header("Visual Smooth")]
    public float playerFollowSpeed = 20f;

    // Cached smooth path
    private readonly List<Vector3> cachedPath = new List<Vector3>(128);
    int lockedSize = -1;
    int maxCornerChange = 1; // toleransi

    // Timing
    private float timer;

    // Optimization
    private Vector3 lastPlayerPos;

    // Visual smoothing
    private Vector3 smoothPlayerPos;

    Vector3 CatmullRom(
    Vector3 p0,
    Vector3 p1,
    Vector3 p2,
    Vector3 p3,
    float t)
    {
        return 0.5f * (
            (2f * p1) +
            (-p0 + p2) * t +
            (2f * p0 - 5f * p1 + 4f * p2 - p3) * t * t +
            (-p0 + 3f * p1 - 3f * p2 + p3) * t * t * t
        );
    }
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

        smoothPlayerPos = player.position;

        line.positionCount = 0;
        line.widthMultiplier = 0.2f;

        SetupLineRenderer();

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
        if (player == null || questTarget == null || agent == null)
            return;

        smoothPlayerPos = Vector3.Lerp(
            smoothPlayerPos,
            agent.nextPosition,
            Time.deltaTime * playerFollowSpeed
        );

        timer += Time.deltaTime;

        if (timer < refreshRate)
            return;

        timer = 0f;


        if ((player.position - lastPlayerPos).sqrMagnitude >= recalcDistance * recalcDistance)
        {
            lastPlayerPos = player.position;
            CalculateAndCachePath();
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
     void SetupLineRenderer()
    {
        line.loop = false;
        line.useWorldSpace = true;

        line.textureMode = LineTextureMode.Tile;
        line.alignment = LineAlignment.View;

        line.numCornerVertices = 6;
        line.numCapVertices = 6;

        line.shadowCastingMode =
            UnityEngine.Rendering.ShadowCastingMode.Off;

        line.receiveShadows = false;

        line.motionVectorGenerationMode =
            MotionVectorGenerationMode.ForceNoMotion;
    }
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

        line.SetPosition(
            0,
            smoothPlayerPos + Vector3.up * lineHeight
        );

        for (int i = 1; i < lockedSize; i++)
        {
            int index = Mathf.Min(i, cachedPath.Count - 1);
            Vector3 pos = cachedPath[index];

            // 🔥 Anchor ke player (hilangkan gap)
            // if (i == 0)
            // {
            //     pos = player.position + Vector3.up * 0.05f;
            // }

            line.SetPosition(i, pos);
        }
    }

    void CalculateAndCachePath()
    {
        NavMeshPath navPath = new NavMeshPath();

        if (!NavMesh.CalculatePath(
            agent.transform.position,
            questTarget.position,
            NavMesh.AllAreas,
            navPath))
            return;

        if (navPath.status != NavMeshPathStatus.PathComplete)
        {
            // Optional: still draw partial path if you want
            return;
        }

        cachedPath.Clear();

        Vector3[] corners = navPath.corners;

        if (corners.Length < 2)
            return;

        // ===== SMOOTH SPLINE =====

        for (int i = 0; i < corners.Length - 1; i++)
        {
            Vector3 p0 =
                i == 0 ? corners[i] : corners[i - 1];

            Vector3 p1 = corners[i];

            Vector3 p2 = corners[i + 1];

            Vector3 p3 =
                i + 2 < corners.Length
                ? corners[i + 2]
                : p2;

            int resolution = 6;

            for (int j = 0; j < resolution; j++)
            {
                float t = j / (float)resolution;

                Vector3 point = CatmullRom(p0, p1, p2, p3, t);

                // Snap back to NavMesh
                if (NavMesh.SamplePosition(
                    point,
                    out NavMeshHit hit,
                    1f,
                    NavMesh.AllAreas))
                {
                    cachedPath.Add(hit.position + Vector3.up * lineHeight);
                }
            }
        }

        cachedPath.Add(
            corners[corners.Length - 1] +
            Vector3.up * lineHeight
        );

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

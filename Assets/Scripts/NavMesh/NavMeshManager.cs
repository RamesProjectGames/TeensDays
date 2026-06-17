using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.AI.Navigation;
using System.Linq;
using UnityEngine.AI;

public class NavMeshManager : MonoBehaviour
{
    public static NavMeshManager Instance;
    public List<GameObject> navMeshSurfaces;
    void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
    }
    void Start()
    {
        var surfaces = FindObjectsByType<NavMeshSurface>(FindObjectsInactive.Include,FindObjectsSortMode.None);
        foreach (var surface in surfaces)
        {
            navMeshSurfaces.Add(surface.gameObject);
        }
    }

    public void RebuildSurfaceOnCertainArea(string areaName)
    {
        foreach (var surface in navMeshSurfaces)
        {
            if(surface.TryGetComponent<NavMeshModifierVolume>(out var modifier))
            {
                if(modifier.area == NavMesh.GetAreaFromName("areaName"))
                {
                    surface.GetComponent<NavMeshSurface>().BuildNavMesh();
                }
            }
        }
    }
    public void RebuildNavMesh()
    {
        foreach (var surface in navMeshSurfaces)
        {
            surface.GetComponent<NavMeshSurface>().BuildNavMesh();
        }
    }
}

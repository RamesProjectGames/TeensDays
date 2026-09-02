using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [Header("Objects Variables")]
    public GameObject[] objectToSpawn;
    public Transform objectParent;
    [Header("Numeric Variable")]
    public int totalPool = 20;
    public Vector2 durationPerSpawn;
    public Vector3 spawnArea = new Vector3(10, 2, 10);
    public Vector3 spawnAreaPivot = new Vector3(0,0,0);
    float spawnTimer;
    public bool canStack;
    [Range(0f, 100f)]
    public float stackChance = 25f;


    public List<GameObject> objects = new List<GameObject>();
    // Start is called before the first frame update
    void Start()
    {
        InstantiateObject();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void InstantiateObject()
    {

        foreach (GameObject prefab in objectToSpawn)
        {
            for (int i = 0; i < totalPool; i++)
            {
                GameObject obj = Instantiate(prefab, Vector3.zero, Quaternion.identity, objectParent);
                obj.SetActive(false);
                objects.Add(obj);
            }
        }
        //for (int i = 0; i < totalPool; i++)
        //{            
        //    GameObject spawnPool = Instantiate(objectToSpawn, Vector3.zero ,Quaternion.identity, objectParent);
        //    spawnPool.SetActive(false);
        //    objects.Add(spawnPool);
        //}
    }
    [ContextMenu("Debug/Pool Objects")]
    public void PoolObjects()
    {
        PoolObjects(totalPool);
    }

    public void PoolObjects(int totalToPool)
    {
        if (objects == null || objects.Count == 0)
        {
            return;
        }

        int numberOfObject = Mathf.Clamp(totalToPool, 0, objects.Count);
        for (int i = 0; i < numberOfObject; i++)
        {
            if (objects[i].activeInHierarchy)
            {
                continue;
            }

            if (canStack && ShouldStack())
            {
                if (i > 0)
                {
                    objects[i].transform.position = GetRandomStackedPos(objects[i - 1]);
                }
                else
                {
                    objects[i].transform.position = GetRandomPos();
                }
            }
            else
            {
                objects[i].transform.position = GetRandomPos();
            }

            objects[i].SetActive(true);
        }
    }
    public void ActivateAllObjects()
    {
        for (int i = 0; i < totalPool; i++)
        {
            if(objects[i].activeInHierarchy)
            {
                continue;
            }
            objects[i].SetActive(true);
        }
    }
    [ContextMenu("Debug/Deactivate All Objects")]
    public void DeactivateAllObjects()
    {
        foreach (GameObject obj in objects)
        {
            obj.SetActive(false);
        }
    }
    
    public Vector3 GetRandomPos()
    {
        Vector3 center = transform.TransformPoint(spawnAreaPivot);

        Vector3 offset = new Vector3(
            Random.Range(-spawnArea.x * 0.5f, spawnArea.x * 0.5f),
            Random.Range(-spawnArea.y * 0.5f, spawnArea.y * 0.5f),
            Random.Range(-spawnArea.z * 0.5f, spawnArea.z * 0.5f)
        );

        return center + transform.rotation * offset;
    }
    public Vector3 GetRandomStackedPos(GameObject basePosition)
    {
        Vector3 center = basePosition.transform.position;

        Vector3 offset = new Vector3(
            basePosition.transform.position.x ,
            basePosition.transform.position.y + basePosition.transform.localScale.y,
            basePosition.transform.position.z
        );

        return center + transform.rotation * offset;
    }
    public bool ShouldStack()
    {
        float percentChance = Mathf.Clamp(stackChance, 0f, 100f);
        return Random.value * 100f < percentChance;
    }
    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        Matrix4x4 matrix = Matrix4x4.TRS(
            transform.TransformPoint(spawnAreaPivot),
            transform.rotation,
            Vector3.one
        );

        Gizmos.matrix = matrix;

        Gizmos.DrawWireCube(Vector3.zero, spawnArea);
        Gizmos.DrawSphere(Vector3.zero, 0.1f);
    }
}

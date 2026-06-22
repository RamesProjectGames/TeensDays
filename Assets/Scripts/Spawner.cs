using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [Header("Objects Variables")]
    public GameObject objectToSpawn;
    public Transform objectParent;
    [Header("Numeric Variable")]
    public int totalPool = 20;
    public Vector2 durationPerSpawn;
    public Vector3 spawnArea = new Vector3(10, 2, 10);
    public Vector3 spawnAreaPivot = new Vector3(0,0,0);
    float spawnTimer;


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
        for (int i = 0; i < totalPool; i++)
        {            
            GameObject spawnPool = Instantiate(objectToSpawn, Vector3.zero ,Quaternion.identity, objectParent);
            spawnPool.SetActive(false);
            objects.Add(spawnPool);
        }
    }
    public void PoolObejct(int totalToPool)
    {
        int numberOfObject = Mathf.Min(totalPool, totalToPool);
        for (int i = 0; i < totalToPool; i++)
        {
            if(objects[i].activeInHierarchy)
            {
                continue;
            }
            objects[i].transform.position = GetRandomPos();
            objects[i].SetActive(true);
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

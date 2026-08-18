using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrafficManager : MonoBehaviour
{
    [System.Serializable]
    public class CarPrefabData
    {
        public GameObject prefab;

        [Min(1)]
        public int poolAmount = 5;
    }

    [Header("CAR PREFABS")]
    public CarPrefabData[] carPrefabs;

    [Header("TRAFFIC NODES")]
    public TrafficNode[] spawnNodes;

    public TrafficNode[] destinationNodes;

    [Header("SPAWN SETTINGS")]
    public float minSpawnDelay = 1.5f;
    public float maxSpawnDelay = 4f;

    [Header("CAR SPEED")]
    public float minSpeed = 4f;
    public float maxSpeed = 7f;

    [Header("ROTATION")]
    public float rotationSpeed = 5f;

    private List<TrafficCar> pool =
        new List<TrafficCar>();

    private void Start()
    {
        CreatePool();

        StartCoroutine(SpawnRoutine());
    }

    private void CreatePool()
    {
        foreach (CarPrefabData data in carPrefabs)
        {
            if (data.prefab == null)
                continue;

            for (int i = 0; i < data.poolAmount; i++)
            {
                GameObject car =
                    Instantiate(
                        data.prefab,
                        transform
                    );

                TrafficCar trafficCar =
                    car.GetComponent<TrafficCar>();

                if (trafficCar == null)
                {
                    trafficCar =
                        car.AddComponent<TrafficCar>();
                }

                car.SetActive(false);

                pool.Add(trafficCar);
            }
        }
    }

    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            float delay =
                Random.Range(
                    minSpawnDelay,
                    maxSpawnDelay
                );

            yield return new WaitForSeconds(delay);

            SpawnRandomCar();
        }
    }

    private void SpawnRandomCar()
    {
        Debug.Log("=== TRY SPAWN CAR ===");

        TrafficCar car = GetAvailableCar();

        if (car == null)
        {
            Debug.LogWarning("Tidak ada car yang tersedia di pool!");
            return;
        }

        TrafficNode spawn = GetRandomSpawnNode();

        if (spawn == null)
        {
            Debug.LogWarning("Spawn Node NULL!");
            return;
        }

        Debug.Log("Spawn Node: " + spawn.name);

        TrafficNode destination =
            GetRandomDestination(spawn);

        if (destination == null)
        {
            Debug.LogWarning("Destination NULL!");
            return;
        }

        Debug.Log(
            "Destination: " +
            destination.name
        );

        List<TrafficNode> route =
            FindRoute(
                spawn,
                destination
            );

        if (route == null)
        {
            Debug.LogWarning(
                "ROUTE TIDAK DITEMUKAN: " +
                spawn.name +
                " -> " +
                destination.name
            );

            return;
        }

        Debug.Log(
            "Route ditemukan. Jumlah node: " +
            route.Count
        );

        float speed =
            Random.Range(
                minSpeed,
                maxSpeed
            );

        car.gameObject.SetActive(true);

        car.Initialize(
            this,
            route,
            speed,
            rotationSpeed
        );
        //TrafficCar car =
        //    GetAvailableCar();

        //if (car == null)
        //    return;

        //TrafficNode spawn =
        //    GetRandomSpawnNode();

        //if (spawn == null)
        //    return;

        //TrafficNode destination =
        //    GetRandomDestination(spawn);

        //if (destination == null)
        //    return;

        //List<TrafficNode> route =
        //    FindRoute(
        //        spawn,
        //        destination
        //    );

        //if (route == null ||
        //    route.Count < 2)
        //    return;

        //float speed =
        //    Random.Range(
        //        minSpeed,
        //        maxSpeed
        //    );

        //car.gameObject.SetActive(true);

        //car.Initialize(
        //    this,
        //    route,
        //    speed,
        //    rotationSpeed
        //);
    }

    private TrafficCar GetAvailableCar()
    {
        List<TrafficCar> availableCars =
            new List<TrafficCar>();

        foreach (TrafficCar car in pool)
        {
            if (!car.gameObject.activeSelf)
            {
                availableCars.Add(car);
            }
        }

        if (availableCars.Count == 0)
            return null;

        return availableCars[
            Random.Range(
                0,
                availableCars.Count
            )
        ];
    }

    private TrafficNode GetRandomSpawnNode()
    {
        if (spawnNodes == null ||
            spawnNodes.Length == 0)
            return null;

        return spawnNodes[
            Random.Range(
                0,
                spawnNodes.Length
            )
        ];
    }

    private TrafficNode GetRandomDestination(
        TrafficNode spawn)
    {
        List<TrafficNode> available =
            new List<TrafficNode>();

        foreach (TrafficNode node in destinationNodes)
        {
            if (node == null)
                continue;

            if (node == spawn)
                continue;

            available.Add(node);
        }

        if (available.Count == 0)
            return null;

        return available[
            Random.Range(
                0,
                available.Count
            )
        ];
    }

    public void ReturnCarToPool(
        GameObject car)
    {
        car.SetActive(false);
    }

    private List<TrafficNode> FindRoute(
        TrafficNode start,
        TrafficNode destination)
    {
        Queue<TrafficNode> queue =
            new Queue<TrafficNode>();

        Dictionary<
            TrafficNode,
            TrafficNode
        > previous =
            new Dictionary<
                TrafficNode,
                TrafficNode
            >();

        queue.Enqueue(start);

        previous[start] = null;

        while (queue.Count > 0)
        {
            TrafficNode current =
                queue.Dequeue();

            if (current == destination)
                break;

            foreach (
                TrafficNode next
                in current.connections)
            {
                if (next == null)
                    continue;

                if (previous.ContainsKey(next))
                    continue;

                previous[next] = current;

                queue.Enqueue(next);
            }
        }

        if (!previous.ContainsKey(destination))
            return null;

        List<TrafficNode> route =
            new List<TrafficNode>();

        TrafficNode currentNode =
            destination;

        while (currentNode != null)
        {
            route.Add(currentNode);

            currentNode =
                previous[currentNode];
        }

        route.Reverse();

        return route;
    }
}

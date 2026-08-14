using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrafficCar : MonoBehaviour
{
    private TrafficManager manager;

    private List<TrafficNode> route;

    private int currentNodeIndex;

    private float speed;

    private float rotationSpeed;

    private bool isDriving;

    public void Initialize(
        TrafficManager manager,
        List<TrafficNode> route,
        float speed,
        float rotationSpeed)
    {
        this.manager = manager;
        this.route = route;
        this.speed = speed;
        this.rotationSpeed = rotationSpeed;

        currentNodeIndex = 0;
        isDriving = true;

        if (route == null || route.Count == 0)
        {
            ReturnToPool();
            return;
        }

        transform.position = route[0].transform.position;

        if (route.Count > 1)
        {
            Vector3 direction =
                route[1].transform.position -
                route[0].transform.position;

            direction.y = 0f;

            if (direction.sqrMagnitude > 0.01f)
            {
                transform.rotation =
                    Quaternion.LookRotation(direction);
            }
        }
    }

    private void Update()
    {
        if (!isDriving)
            return;

        if (route == null ||
            route.Count == 0)
            return;

        Move();
    }

    private void Move()
    {
        if (currentNodeIndex >= route.Count - 1)
        {
            ReturnToPool();
            return;
        }

        TrafficNode targetNode =
            route[currentNodeIndex + 1];

        Vector3 targetPosition =
            targetNode.transform.position;

        Vector3 direction =
            targetPosition - transform.position;

        direction.y = 0f;

        float distance =
            direction.magnitude;

        if (distance > 0.01f)
        {
            Vector3 moveDirection =
                direction.normalized;

            transform.position +=
                moveDirection *
                speed *
                Time.deltaTime;

            Quaternion targetRotation =
                Quaternion.LookRotation(moveDirection);

            transform.rotation =
                Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    rotationSpeed *
                    Time.deltaTime
                );
        }

        if (distance < 0.3f)
        {
            currentNodeIndex++;
        }
    }

    private void ReturnToPool()
    {
        isDriving = false;

        manager.ReturnCarToPool(gameObject);
    }
}

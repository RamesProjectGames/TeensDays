using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class RunSystem : MonoBehaviour
{
    public bool isRunning;

    public float moveRun;
    public void Run()
    {
        if (isRunning)
        {
            PlayerMovement.Instance.moveSpeed = 15f;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Ground"))
        {
            isRunning = true;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        PlayerMovement.Instance.moveSpeed = 15f;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        PlayerMovement.Instance.moveSpeed = 5f;
    }
}

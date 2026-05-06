using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class RunSystem : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public bool isRunning;

    public void StartRunning()
    {
        PlayerMovement.Instance.moveSpeed = 7f;
        isRunning = true;
        PlayerMovement.Instance.animator.SetBool("Running", true);
    }

    public void StopRunning()
    {
        PlayerMovement.Instance.moveSpeed = 2f;
        isRunning = false;
        PlayerMovement.Instance.animator.SetBool("Running", false);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        StartRunning();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        StopRunning();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelRotation : MonoBehaviour
{
    [Header("Wheel")]
    public Transform[] wheels;

    [Header("Rotation")]
    public float rotationSpeed = 360f;

    private void Update()
    {
        foreach (Transform wheel in wheels)
        {
            wheel.Rotate(Vector3.right * rotationSpeed * Time.deltaTime, Space.Self);
        }
    }
}

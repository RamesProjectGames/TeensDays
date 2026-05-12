using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InspectObject : MonoBehaviour, IDragHandler
{
    [SerializeField] private float rotationSpeed = 0.5f;
    
    public bool Horizontal, Vertical;
    [HideInInspector]
    public string inspectGuid;

    public void OnDrag(PointerEventData eventData)
    {
        var data =
            InspectManager.Instance.GetInspectObject(inspectGuid);

        if (data == null || data.spawnedObject == null)
            return;

        Transform target = data.spawnedObject;

        float horizontal =
            -eventData.delta.x * rotationSpeed;

        float vertical =
            eventData.delta.y * rotationSpeed;

        if(!Horizontal) horizontal = 0;
        if(!Vertical) vertical = 0;
        // HORIZONTAL AROUND LOCAL Y
        target.Rotate(
            Vector3.up,
            horizontal,
            Space.Self
        );

        // VERTICAL AROUND CAMERA RIGHT
        target.Rotate(
            Camera.main.transform.right,
            vertical,
            Space.World
        );
    }
}

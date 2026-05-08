using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FreeLookCam : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    Image imgCameControl;
    [Header("Cinemachine")]
    [SerializeField] CinemachineFreeLook camFreeLook;

    [Header("Settings")]
    [SerializeField] private float xSensitivity = 0.05f;
    [SerializeField] private float ySensitivity = 0.001f;

    private Vector2 lastDragPos;
    private bool isDragging;
    //[SerializeField] CinemachineFreeLook camFreeLook;
    //string strMouseX = "Mouse X";
    //string strMouseY = "Mouse Y";

    // Start is called before the first frame update
    void Start()
    {
        imgCameControl = GetComponent<Image>();

        if (imgCameControl != null)
        {
            imgCameControl.color = new Color(1, 1, 1, 0);

            // penting supaya UI bisa detect drag
            imgCameControl.raycastTarget = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;

        Vector2 currentPos = eventData.position;
        Vector2 delta = currentPos - lastDragPos;

        // rotate horizontal
        camFreeLook.m_XAxis.Value += delta.x * xSensitivity;

        // rotate vertical
        camFreeLook.m_YAxis.Value -= delta.y * ySensitivity;

        lastDragPos = currentPos;

    }

    public void OnPointerDown(PointerEventData eventData)
    {
        //OnDrag(eventData);

        isDragging = true;
        lastDragPos = eventData.position;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isDragging = false;
        //camFreeLook.m_XAxis.m_InputAxisName = null;
        //camFreeLook.m_YAxis.m_InputAxisName= null;

        //camFreeLook.m_XAxis.m_InputAxisValue = 0;
        //camFreeLook.m_YAxis.m_InputAxisValue= 0;
    }
}

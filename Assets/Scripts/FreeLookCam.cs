using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FreeLookCam : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    Image imgCameControl;
    [SerializeField] CinemachineFreeLook camFreeLook;
    string strMouseX = "Mouse X";
    string strMouseY = "Mouse Y";

    // Start is called before the first frame update
    void Start()
    {
        imgCameControl = GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(imgCameControl.rectTransform, eventData.position, eventData.enterEventCamera, out Vector2 posOut))
        {
            camFreeLook.m_XAxis.m_InputAxisName = strMouseX;
            camFreeLook.m_YAxis.m_InputAxisName = strMouseY;
        }
        
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnDrag(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        camFreeLook.m_XAxis.m_InputAxisName = null;
        camFreeLook.m_YAxis.m_InputAxisName= null;

        camFreeLook.m_XAxis.m_InputAxisValue = 0;
        camFreeLook.m_YAxis.m_InputAxisValue= 0;
    }
}

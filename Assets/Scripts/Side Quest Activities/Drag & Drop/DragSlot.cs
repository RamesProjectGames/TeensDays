using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragSlot : MonoBehaviour
{
    public int SlotIndex;
    public DragItem CurrentItem;
    public RectTransform RectTransform { get; private set; }

    void Start()
    {
        RectTransform = GetComponent<RectTransform>();
        
    }
}

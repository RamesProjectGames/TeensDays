using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryUIManager : MonoBehaviour
{
    public InventoryItemUI[] items;

    public void RefreshAll()
    {
        foreach (var item in items)
        {
            item.Refresh();
        }
    }
}

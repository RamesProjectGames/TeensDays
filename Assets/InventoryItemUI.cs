using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryItemUI : MonoBehaviour
{
    public string itemId;

    public Button useButton; 
    //public GameObject lockIcon; // opsional (kalau mau ada icon gembok)

    public void Setup(string _itemId)
    {
        itemId = _itemId;

        Refresh();
    }

    public void Refresh()
    {
        bool owned = InventoryManager.Instance.HasItem(itemId);

        useButton.interactable = owned;

        if (owned)
        {
            useButton.GetComponentInChildren<TMP_Text>().text = "Use";
        }
        else
        {
            useButton.GetComponentInChildren<TMP_Text>().text = "Locked";
        }

        //if (lockIcon != null)
        //    lockIcon.SetActive(!owned);
    }
}

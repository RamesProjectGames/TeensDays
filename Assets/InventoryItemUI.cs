using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryItemUI : MonoBehaviour
{
    public string itemId;
    public string rarity;

    public Button useButton; 
    public Image rarityImage;
    public Image iconImage;
    public TMP_Text itemNameText;

    //public GameObject lockIcon; // opsional (kalau mau ada icon gembok)

    public void Setup(string _itemId, string rarity, Sprite _iconSprite, string _itemName, Sprite _raritySprite)
    {
        itemId = _itemId;
        this.rarity = rarity;
        iconImage.sprite = _iconSprite;
        itemNameText.text = _itemName;
        rarityImage.sprite = _raritySprite;

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

        useButton.onClick.RemoveAllListeners();

        useButton.onClick.AddListener(() =>
        {
            if (owned)
            {
                // Lakukan aksi saat item digunakan
                Debug.Log("Using item: " + itemId);
                CostumeManager.Instance.ApplyProfileCostume(itemId);
            }
            else
            {
                Debug.Log("Item is locked: " + itemId);
                // Tambahkan logika untuk item terkunci di sini
            }
        });

        //if (lockIcon != null)
        //    lockIcon.SetActive(!owned);
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    //public ShopItemList currentItemList;
    public static ShopManager instance;
    public Button[] ShopBtns;
    public Sprite[] onClickBtns;
    public Sprite[] onUpBtns2;
    public GameObject[] kontents;
    public GameObject filterButton;
    public bool[] shopChecked;
    public ShopItemList currentItemList;
    public ScrollRect currentScrollRect;

    public int selectedIndex;
    //public int filterIndex;

    public ItemUI[] itemCards; // drag prefab UI item di Inspector

    [Header("Filter Settings")]
    public string[] filterRarity;

    public CostumeShopPreview costumePreview;

    private void Awake()
    {
        instance = this;

        //MakeJsonData();
    }

    void Start()
    {
        //LoadShopItems();

        for (int i = 0; i < itemCards.Length; i++)
        {
            itemCards[i].costumePreview = costumePreview;
        }

        for (int i = 0; i < ShopBtns.Length; i++)
        {
            int index = i;
            ShopBtns[i].onClick.AddListener(() => OnTabClicked(index));
        }

        OnTabClicked(0); // Pilih tab pertama saat mulai
    }

    public void OnTabClicked(int index)
    {
        selectedIndex = index;

        for (int i = 0; i < ShopBtns.Length; i++)
        {
            Image buttonImage = ShopBtns[i].GetComponent<Image>();
            TextMeshProUGUI buttonText = ShopBtns[i].GetComponentInChildren<TextMeshProUGUI>();

            if (i == index)
            {
                buttonImage.sprite = onClickBtns[i];
                currentScrollRect.content = kontents[i].GetComponent<RectTransform>();
                kontents[i].SetActive(true);
                if (kontents[1].activeInHierarchy)
                {
                    filterButton.SetActive(true);
                }
                shopChecked[i] = true;
            }
            else
            {
                buttonImage.sprite = onUpBtns2[i];
                kontents[i].SetActive(false);
                if (!kontents[1].activeInHierarchy)
                {
                    filterButton.SetActive(false);
                }
                shopChecked[i] = false;
            }
        }
    }

    public void OnTabClickedFilter(int _filterIndex)
    {
        //filterIndex = _filterIndex;

        ApplyFilter(filterRarity[_filterIndex]);
    }

    #region oldScript
    //public void LoadShopItems()
    //{
    //    string path = Path.Combine(Application.streamingAssetsPath, "shop_items.json");

    //    if (File.Exists(path))
    //    {
    //        string jsonData = File.ReadAllText(path);
    //        string wrappedJson = "{ \"items\": " + jsonData + "}";
    //        currentItemList = JsonUtility.FromJson<ShopItemList>(wrappedJson);

    //        for (int i = 0; i < currentItemList.items.Length && i < itemCards.Length; i++)
    //        {
    //            itemCards[i].SetItem(
    //                currentItemList.items[i].itemId,
    //                currentItemList.items[i].name,
    //                currentItemList.items[i].description,
    //                currentItemList.items[i].price,
    //                currentItemList.items[i].priceMoney,
    //                currentItemList.items[i].isDiamondPayment,
    //                currentItemList.items[i].rarity);
    //        }
    //    }
    //}
    #endregion

    void ApplyFilter(string rarity)
    {
        for (int i = 0; i < itemCards.Length; i++)
        {
            if (rarity.ToLower() == "all")
            {
                itemCards[i].gameObject.SetActive(true);
            }
            else
            {
                bool match = itemCards[i].rarityType.ToLower() == rarity.ToLower();
                itemCards[i].gameObject.SetActive(match);
            }
        }
    }

    public void BuyItem(string itemId)
    {
        if (currentItemList == null || currentItemList.items == null)
        {
            Debug.LogError("Shop data belum terload!");
            return;
        }

        ShopItem item = currentItemList.items.FirstOrDefault(i => i.itemId == itemId);

        if (item == null)
        {
            Debug.LogError("Item tidak ditemukan: " + itemId);
            return;
        }

        if (InventoryManager.Instance == null)
        {
            Debug.LogError("InventoryManager tidak ada di scene!");
            return;
        }

        if (InventoryManager.Instance.HasItem(itemId))
        {
            Debug.Log("Item sudah dimiliki!");
            return;
        }

         if (item.isDiamondPayment)
         {
             if (GameManager.Instance.playerData.currDiamond >= item.price)
             {
                 GameManager.Instance.playerData.currDiamond -= item.price;
             }
             else
             {
                 Debug.Log("Diamond tidak cukup!");
                 return;
             }
         }
         else
         {
             if (GameManager.Instance.playerData.currMoney >= item.priceMoney)
             {
                 GameManager.Instance.playerData.currMoney -= item.priceMoney;
             }
             else
             {
                 Debug.Log("Money tidak cukup!");
                 return;
             }
         }

        InventoryManager.Instance.AddItem(itemId);

        var uiManager = FindObjectOfType<InventoryUIManager>();
        if (uiManager != null)
        {
            uiManager.RefreshAll();
        }

        // Save to cloud instead of PlayerPrefs
        GameManager.Instance.SavePlayerDataToCloud();

        Debug.Log("Berhasil beli: " + item.name);
    }



    //public void LoadShopItems()
    //{
    //    string path = Path.Combine(Application.streamingAssetsPath, "shop_items.json");


    //    if (File.Exists(path))
    //    {
    //        string jsonData = File.ReadAllText(path);

    //        // Karena format JSON berupa array, kita bungkus manual agar bisa di-parse
    //        string wrappedJson = "{ \"items\": " + jsonData + "}";

    //        ShopItemList itemList = JsonUtility.FromJson<ShopItemList>(wrappedJson);

    //        for (int i = 0; i < itemList.items.Length && i < itemCards.Length; i++)
    //        {
    //            itemCards[i].SetItem(
    //                itemList.items[i].itemId,
    //                itemList.items[i].name,
    //                itemList.items[i].description,
    //                itemList.items[i].price,
    //                itemList.items[i].priceMoney,
    //                itemList.items[i].isDiamondPayment,
    //                itemList.items[i].rarity);
    //        }
    //    }
    //    else
    //    {
    //        Debug.LogError("File JSON tidak ditemukan di: " + path);
    //    }
    //}

    void MakeJsonData()
    {
        ShopItem data = new ShopItem();

        string json = JsonUtility.ToJson(data);

        string filePath = Path.Combine(Application.streamingAssetsPath, "shop_items.json");
        File.WriteAllText(filePath, json);

        Debug.Log("Game data saved to: " + filePath);
    }
}

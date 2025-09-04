using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    public static ShopManager instance;
    public Button[] ShopBtns;
    public Sprite[] onClickBtns;
    public Sprite[] onUpBtns2;
    public GameObject[] kontents;

    public int selectedIndex;

    public ItemUI[] itemCards; // drag prefab UI item di Inspector

    private void Awake()
    {
        instance = this;

        //MakeJsonData();
    }

    void Start()
    {
        LoadShopItems();

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
                kontents[i].SetActive(true);
            }
            else
            {
                buttonImage.sprite = onUpBtns2[i];
                kontents[i].SetActive(false);
            }
        }
    }

    public void LoadShopItems()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "shop_items.json");


        if (File.Exists(path))
        {
            string jsonData = File.ReadAllText(path);

            // Karena format JSON berupa array, kita bungkus manual agar bisa di-parse
            string wrappedJson = "{ \"items\": " + jsonData + "}";

            ShopItemList itemList = JsonUtility.FromJson<ShopItemList>(wrappedJson);

            for (int i = 0; i < itemList.items.Length && i < itemCards.Length; i++)
            {
                itemCards[i].SetItem(itemList.items[i].name, itemList.items[i].description, itemList.items[i].price, itemList.items[i].priceMoney, itemList.items[i].isDiamondPayment);
            }
        }
        else
        {
            Debug.LogError("File JSON tidak ditemukan di: " + path);
        }
    }

    void MakeJsonData()
    {
        ShopItem data = new ShopItem();

        string json = JsonUtility.ToJson(data);

        string filePath = Path.Combine(Application.streamingAssetsPath, "shop_items.json");
        File.WriteAllText(filePath, json);

        Debug.Log("Game data saved to: " + filePath);
    }
}

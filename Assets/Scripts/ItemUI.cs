using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ItemUI : MonoBehaviour
{
    public static ItemUI Instance;

    [Header("Panel Shop Confirmation")]
    public GameObject confirmationPanel;
    public GameObject completePurchased;
    public GameObject cancelPurchased;

    private int price;
    private int priceMoney;

    [Header("Shop Panel")]
    public Image currImage;
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI itemDescText;
    public TextMeshProUGUI priceText;
    public Image rarityFrame;
    public string rarityType;
    public string itemId;
    public Button buyButton;

    public Sprite imageDiamond;
    public Sprite imageMoney;
    public bool isDiamondPayment;
    public bool isAlreadyPurchased;

    private void Awake()
    {
        Instance = this;
    }

    public void SetItem(string _itemId,string name, string desc, int priceValue, int _priceMoney, bool _isDiamondPayment, string rarity)
    {
        itemNameText.text = name;
        itemDescText.text = desc;
        isDiamondPayment = _isDiamondPayment;
        rarityType = rarity;
        itemId = _itemId;
        var img = currImage.sprite;

        switch (rarity.ToLower())
        {
            case "rare": rarityFrame.color = Color.blue; break;
            case "epic": rarityFrame.color = new Color(0.6f, 0f, 1f); break; // ungu
            case "legend": rarityFrame.color = new Color(1f, 0.5f, 0f); break; // oranye
            case "mythic": rarityFrame.color = Color.red; break;
            default: rarityFrame.color = Color.white; break;
        }

        if (isDiamondPayment)
        {
            price = priceValue;
            priceText.text = price.ToString();
            currImage.sprite = imageDiamond;
        }
        else
        {
            currImage.sprite = imageMoney;
            priceMoney = _priceMoney;
            priceText.text = priceMoney.ToString();
        }   

        buyButton.onClick.RemoveAllListeners();

        // ✅ simpan dulu ke variable lokal agar tidak tertimpa
        string thisItemId = _itemId;
        buyButton.onClick.AddListener(() => BuyItem(thisItemId)); //Debug.Log(_itemId)
    }

    void BuyItem(string itemId)
    {
        Debug.Log(itemId);

        string path = Path.Combine(Application.streamingAssetsPath, "shop_items.json");
        string jsonData = File.ReadAllText(path);
        string wrappedJson = "{ \"items\": " + jsonData + "}";
        ShopItemList itemList = JsonUtility.FromJson<ShopItemList>(wrappedJson);

        for (int i = 0; i < itemList.items.Length; i++)
        {
            ShopItem item = itemList.items[i];
            if (itemId == item.itemId)
            {
                if (item.isDiamondPayment)
                {
                    int playerCurrency = GameManager.Instance.currDiamond;
                    if (playerCurrency >= item.price)
                    {
                        playerCurrency -= item.price;
                        GameManager.Instance.currDiamond = playerCurrency;
                        PlayerPrefs.SetInt("Diamond", playerCurrency);
                        Debug.Log("Berhasil beli item: " + item.name);
                        completePurchased.SetActive(true);
                    }
                    else
                    {
                        Debug.Log("Currency tidak cukup!");
                        cancelPurchased.SetActive(true);
                    }
                }
                else
                {
                    int _playerCurrency = GameManager.Instance.currMoney;
                    if (_playerCurrency >= item.priceMoney)
                    {
                        _playerCurrency -= item.priceMoney;
                        GameManager.Instance.currMoney = _playerCurrency;
                        PlayerPrefs.SetInt("Money", _playerCurrency);
                        Debug.Log("Berhasil beli item: " + item.name);
                        completePurchased.SetActive(true);
                    }
                    else
                    {
                        Debug.Log("Currency tidak cukup!");
                        cancelPurchased.SetActive(true);
                    }
                }
                PlayerPrefs.Save();
                break;
            }
        }
    }
}

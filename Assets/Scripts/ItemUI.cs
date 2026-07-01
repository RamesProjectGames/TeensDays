using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static CostumeChanger;
//using static UnityEditor.Progress;

public class ItemUI : MonoBehaviour
{
    //public static ItemUI Instance;

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
    public Image iconImage;
    public Sprite[] imageRarityFrame;
    public string rarityType;
    public string itemId;
    public Button buyButton;
    public Button previewButton;

    public Sprite imageDiamond;
    public Sprite imageMoney;
    public bool isDiamondPayment;
    public bool isAlreadyPurchased;
    public CostumeShopPreview costumePreview;

    //private void Awake()
    //{
    //    Instance = this;
    //}

    public void SetItem(string _itemId,string name, string desc, int priceValue, int _priceMoney, bool _isDiamondPayment, string rarity, Sprite icon)
    {
        itemNameText.text = name;
        itemDescText.text = desc;
        isDiamondPayment = _isDiamondPayment;
        rarityType = rarity;
        itemId = _itemId;
        var img = currImage.sprite;
        if(iconImage != null) iconImage.sprite = icon;

        switch (rarity.ToLower())
        {
            case "common": rarityFrame.sprite = imageRarityFrame[0]; break;
            case "rare": rarityFrame.sprite = imageRarityFrame[1]; break;
            case "epic": rarityFrame.sprite = imageRarityFrame[2]; break;
            case "legend": rarityFrame.sprite = imageRarityFrame[3]; break;
            //case "mythic": rarityFrame.sprite = imageRarityFrame[4]; break;
            default: rarityFrame.sprite = imageRarityFrame[0]; break;
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

        previewButton.onClick.RemoveAllListeners();

        previewButton.onClick.AddListener(() =>
        {
            costumePreview.ShowPreview(itemId);
        });

        buyButton.onClick.RemoveAllListeners();

        string thisItemId = _itemId;
        buyButton.onClick.AddListener(() => {
            ShopManager.instance.BuyItem(itemId);
        });
    }

    


}

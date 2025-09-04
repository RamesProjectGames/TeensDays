using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemUI : MonoBehaviour
{



    [Header("Panel Shop Confirmation")]
    public GameObject confirmationPanel;
    public GameObject completePurchased;
    public GameObject cancelPurchased;

    private int price;
    private int priceMoney;


    public Image currImage;
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI itemDescText;
    public TextMeshProUGUI priceText;
    public Button buyButton;

    public Sprite imageDiamond;
    public Sprite imageMoney;
    public bool isDiamondPayment;
    public void SetItem(string name, string desc, int priceValue, int _priceMoney, bool _isDiamondPayment)
    {
        itemNameText.text = name;
        itemDescText.text = desc;
        isDiamondPayment = _isDiamondPayment;
        var img = currImage.sprite;

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
        buyButton.onClick.AddListener(() => BuyItem());
    }

    void BuyItem()
    {
        int playerCurrency = PlayerPrefs.GetInt("Diamond", 0);
        int _playerCurrency = PlayerPrefs.GetInt("Money", 0);

        if (isDiamondPayment)
        {
            if (playerCurrency >= price)
            {
                GameManager.Instance.currDiamond -= price;
                PlayerPrefs.SetInt("Diamond", playerCurrency);
                Debug.Log("Berhasil beli item: " + itemNameText.text);
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
            if (_playerCurrency >= priceMoney)
            {
                GameManager.Instance.currMoney -= priceMoney;
                PlayerPrefs.SetInt("Diamond", _playerCurrency);
                Debug.Log("Berhasil beli item: " + itemNameText.text);
                completePurchased.SetActive(true);


            }
            else
            {
                Debug.Log("Currency tidak cukup!");
                cancelPurchased.SetActive(true);
            }
        }

            PlayerPrefs.Save();
    }
}

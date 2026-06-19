using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IAPItemUI : MonoBehaviour
{
    public Image icon;
    public TextMeshProUGUI rewardText;
    public Button buyButton;
    public string priceText;
    public string descriptionText;
    public IAPProductKey productKey;
    public void Initilized(string price, string description, string reward, bool isActive, IAPProductKey productKey)
    {
        priceText = price;
        descriptionText = description;
        rewardText.text = reward;
        buyButton.interactable = isActive;
        this.productKey = productKey;
    }
    void Update()
    {
        // icon.sprite = ContentManager.Singleton.GetSprite(productKey.ToString());
    }
    public void SetInteractable(bool isInteractable)
    {
        buyButton.interactable = isInteractable;
    }
    public void SetIcon(Sprite icon)
    {
        if(icon == null) return;
        this.icon.sprite = icon;
    }
    public void SetReward(string reward)
    {
        rewardText.text = reward;
    }
    public void SetClickButton(Action onClick = null)
    {
        buyButton.onClick.AddListener(() =>
        {
            onClick?.Invoke();
        });
    }
}

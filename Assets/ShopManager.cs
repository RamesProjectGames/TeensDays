using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    public Button[] ShopBtns;
    public Sprite[] onClickBtns;
    public Sprite[] onUpBtns2;
    public GameObject[] kontents;

    public int selectedIndex;

    void Start()
    {
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
}

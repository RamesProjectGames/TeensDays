using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestUIManager : MonoBehaviour
{
    [Header("Quest Panels")]
    public Transform panelMainQuestList;
    public Transform panelSubQuestList;
    public Transform panelSideQuestList;

    [Header("Prefabs")]
    public GameObject questItemPrefab;
    public GameObject subQuestItemPrefab;

    [Header("References")]
    public GameObject panelQuests;     // Panel semua quest
    public Image arrowIcon;            // Icon panah
    public Sprite arrowDown;           // Sprite panah ke bawah
    public Sprite arrowUp;             // Sprite panah ke atas

    private bool isOpen = true;

    private void Start()
    {

    }


    // Collapse/expand Objectives
    public void ToggleObjectives()
    {
        isOpen = !isOpen;

        // Aktifkan / nonaktifkan panel quest
        panelQuests.SetActive(isOpen);

        // Ubah sprite panah
        arrowIcon.sprite = isOpen ? arrowDown : arrowUp;
    }
}

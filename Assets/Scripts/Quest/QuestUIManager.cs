using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class QuestUIManager : MonoBehaviour
{
    [Header("Quest Panels")]
    public Transform panelMainQuestList;
    public Transform panelSubQuestList;

    [Header("Prefabs")]
    public GameObject questItemPrefab;

    [Header("References")]
    public GameObject objectivesPanel;

    // Tambahkan quest ke panel sesuai kategori
    public void AddQuest(string questName, bool isMainQuest)
    {
        GameObject newItem = Instantiate(questItemPrefab);

        newItem.GetComponentInChildren<TMP_Text>().text = questName;

        if (isMainQuest)
            newItem.transform.SetParent(panelMainQuestList, false);
        else
            newItem.transform.SetParent(panelSubQuestList, false);
    }

    // Collapse/expand Objectives
    public void ToggleObjectives()
    {
        objectivesPanel.SetActive(!objectivesPanel.activeSelf);
    }
}

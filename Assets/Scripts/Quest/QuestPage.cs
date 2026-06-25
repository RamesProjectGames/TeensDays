using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestPage : MonoBehaviour
{
    public TMP_Text titleText;
    public TMP_Text subtitleText;
    public Button leadButton;
    public Quest quest;
    public void Set(string title, string subtitle, Quest quest = null, Action leadQuest = null)
    {
        titleText.text = title;
        subtitleText.text = subtitle;
        leadButton.onClick.AddListener(() =>
        {
            leadQuest?.Invoke();
        });
        this.quest = quest;
    }
}

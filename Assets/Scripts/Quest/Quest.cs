using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

[System.Serializable]
public class Quest
{
    public TMP_Text questText;
    public string text;
    public int expForQuest;
    public Transform targetTransform;
    public GameObject questObjectAnnoun;
    public GameObject npcObject;
    public bool isDone;

    public List<Quest> subQuests = new List<Quest>(); // Tambahkan ini

    public GameObject questUIObject;   // referensi prefab UI
    public Outline questOutline;       // referensi Outline UI

    public bool IsFullyCompleted()
    {
        // Quest utama selesai jika dirinya dan semua subquest selesai
        return isDone && subQuests.All(sq => sq.isDone);
    }
}
[System.Serializable]
public class QuestData
{
    public string questName;
    public bool isDone;
    public List<QuestData> subQuests = new List<QuestData>();
    public bool IsFullyCompleted()
    {
        // Quest utama selesai jika dirinya dan semua subquest selesai
        return isDone && subQuests.All(sq => sq.isDone);
    }
}

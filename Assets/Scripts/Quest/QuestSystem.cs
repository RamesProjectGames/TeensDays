using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class QuestSystem : MonoBehaviour
{
    public List<Quest> quests = new List<Quest>();
    public List<GameObject> questObject = new List<GameObject>();
    public List<bool> boolQuest = new List<bool>();
    public List<Outline> outlineQuest = new List<Outline>();
    [SerializeField] private int currentQuestIndex = 0;
    [SerializeField] private float blinkSpeed = 2f;

    private void Update()
    {
        UpdateQuestDisplay();
        CheckAutoCompleteQuests();
        UpdateQuestOutlines();
    }

    public void UpdateQuestDisplay()
    {
        foreach (var quest in quests)
        {
            UpdateSingleQuestDisplay(quest);
        }
    }

    private void UpdateSingleQuestDisplay(Quest quest)
    {
        if (quest.isDone)
        {
            quest.questText.text = $"<s>{quest.text}</s>";
        }
        else
        {
            quest.questText.text = quest.text;
        }

        foreach (var subQuest in quest.subQuests)
        {

            if (subQuest.isDone)
            {
                subQuest.questText.text = $"<s>  > {subQuest.text}</s>";
            }
            else
            {
                subQuest.questText.text = $"  > {subQuest.text}";
            }

        }
    }

    public void MarkQuestDone(int questIndex, int subQuestIndex)
    {
        if (questIndex >= 0 && questIndex < quests.Count)
        {
            var quest = quests[questIndex];
            if (subQuestIndex >= 0 && subQuestIndex < quest.subQuests.Count)
            {
                quest.subQuests[subQuestIndex].isDone = true;
                UpdateQuestDisplay();
            }
        }
    }

    public void CheckAutoCompleteQuests()
    {
        if (currentQuestIndex < quests.Count)
        {
            var quest = quests[currentQuestIndex];

            if (!quest.isDone && quest.subQuests.All(sq => sq.isDone))
            {
                quest.isDone = true;
                currentQuestIndex++;
                ActivateQuestObject(currentQuestIndex);
            }
        }
    }

    public void ActivateQuestObject(int index)
    {
        for (int i = 0; i < questObject.Count; i++)
        {
            questObject[i].SetActive(i == index);
        }
    }

    private void UpdateQuestOutlines()
    {
        for (int i = 0; i < outlineQuest.Count; i++)
        {
            if (outlineQuest[i] == null) continue;

            // Boolean aktif + quest index cocok dengan quest aktif
            if (i == currentQuestIndex && questObject[i])
            {
                outlineQuest[i].enabled = true; // tetap nyala

                // PingPong nilai alpha antara 0 → 1
                float alpha = Mathf.PingPong(Time.time * blinkSpeed, 1f);

                // ambil warna lama, lalu ubah alpha
                Color c = outlineQuest[i].OutlineColor;
                c.a = alpha;
                outlineQuest[i].OutlineColor = c;
            }
            else
            {
                outlineQuest[i].enabled = false;
            }
        }
    }
}

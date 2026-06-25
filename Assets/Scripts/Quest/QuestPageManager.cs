using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class QuestPageManager : MonoBehaviour
{
    public static QuestPageManager Instance;
    [Header("Quest Sections")]
    public GameObject QuestPagePanel;
    public Transform questPrefabParent;
    public GameObject mainQuestPrefab;
    public GameObject subQuestPrefab;
    public TMP_Text questTitle;
    public TMP_Text subQuestTitle;
    public TMP_Text questDescription;
    [Header("Reward Sections")]
    public Transform rewardsParent;
    public GameObject rewardPrefab;

    List<GameObject> mainQuestsBox = new List<GameObject>();
    List<GameObject> subQuestBox = new List<GameObject>();
    void Awake()
    {
        Instance = this;
    }
    public void PopulateQuestPage()
    {
        if(QuestSystem.instance == null) return;
        foreach (var mainQuest in QuestSystem.instance.quests)
        {
            if(mainQuest.isDone)
            {
                continue;
            }
            GameObject mainQuestBox = Instantiate(mainQuestPrefab, questPrefabParent);
            var currentSubQuest = SelectSubQuest(mainQuest);
            if(mainQuestBox.TryGetComponent<QuestPage>(out var page))
            {
                page.Set(mainQuest.text,currentSubQuest == null ? "" : currentSubQuest.text,mainQuest, () =>
                {
                    SetPageContents(mainQuest.text,currentSubQuest == null ? "" : currentSubQuest.text,"");
                    QuestSystem.instance.AddNewQuest(mainQuest,true,false,0,false);
                    QuestPagePanel.SetActive(false);                    
                });
            }
            mainQuestsBox.Add(mainQuestBox);
        }
        foreach (var sideQuest in QuestSystem.instance.sideQuests)
        {
            if(sideQuest.isDone)
            {
                continue;
            }
            GameObject sideQuestBox = Instantiate(mainQuestPrefab, questPrefabParent);
            var currentSubQuest = SelectSubQuest(sideQuest);
            if(sideQuestBox.TryGetComponent<QuestPage>(out var page))
            {
                page.Set(sideQuest.text,currentSubQuest == null ? "" : currentSubQuest.text,sideQuest, () =>
                {
                    SetPageContents(sideQuest.text,currentSubQuest == null ? "" : currentSubQuest.text,"");
                    QuestSystem.instance.AddNewQuest(currentSubQuest,false,true,sideQuest.subQuests.FindIndex(x=>x==currentSubQuest),true);
                    QuestPagePanel.SetActive(false);
                });
            }
            subQuestBox.Add(sideQuestBox);
        }
    }
    public void SetPageContents(string title, string subTitle, string desc)
    {
        questTitle.text = title;
        subQuestTitle.text = subTitle;
        questDescription.text = desc;
    }
    public void UpdateQuestPage(bool isMain = true, bool isSide = true)
    {
        if(QuestSystem.instance == null) return;
        foreach (var mainQuest in mainQuestsBox)
        {
            mainQuest.SetActive(!QuestSystem.instance.GetQuest(mainQuest.GetComponent<QuestPage>().quest.text).isDone && isMain);
        }
        foreach (var subQuest in subQuestBox)
        {
            subQuest.SetActive(!QuestSystem.instance.GetQuest(subQuest.GetComponent<QuestPage>().quest.text).isDone && isSide);
        }
    }
    public void ShowQuestAll()
    {
        UpdateQuestPage();
    }
    public void ShowMainQuest()
    {
        UpdateQuestPage(true, false);
    }
    public void ShowSideQuest()
    {
        UpdateQuestPage(false, true);
    }
    public Quest SelectSubQuest(Quest quest)
    {
        if(QuestSystem.instance == null) return null;
        Quest currentSubQuest = null;
        foreach (var subQuest in quest.subQuests)
        {
            if(subQuest.isDone)
            {
               currentSubQuest = subQuest;
               break;
            }
        }
        return currentSubQuest;
    }
}

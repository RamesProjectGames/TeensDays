using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
    public GameObject navigateButton;
    [Header("Reward Sections")]
    public Transform rewardsParent;
    public GameObject rewardPrefab;
    public GameObject noRewardsText;

    List<GameObject> mainQuestsBox = new List<GameObject>();
    List<GameObject> subQuestBox = new List<GameObject>();
    Quest currentQuest = null;
    void Awake()
    {
        Instance = this;
    }
    public void PopulateQuestPage()
    {
        if(QuestSystem.instance == null) return;
        for (int i = 0; i < QuestSystem.instance.quests.Count; i++)
        {
            Quest mainQuest = QuestSystem.instance.quests[i];
            if (mainQuest.isDone)
            {
                continue;
            }
            GameObject mainQuestBox = Instantiate(mainQuestPrefab, questPrefabParent);
            var currentSubQuest = SelectSubQuest(mainQuest);
            if(mainQuestBox.TryGetComponent<QuestPage>(out var page))
            {
                page.Set(mainQuest.text,currentSubQuest == null ? "" : currentSubQuest.text,mainQuest, () =>
                {
                    SetPageContents(mainQuest.text,currentSubQuest == null ? "This Quest Does't Have sub quests" : currentSubQuest.text,mainQuest.description);
                    currentQuest = mainQuest;
                    navigateButton.SetActive(true);
                    PopulateRewards();
                });
            }
            mainQuestBox.SetActive(i==QuestSystem.instance.GetCurrentQuestIndex());
            mainQuestsBox.Add(mainQuestBox);
        }
        foreach (var sideQuest in QuestSystem.instance.sideQuests)
        {
            if(sideQuest.isDone)
            {
                continue;
            }
            GameObject sideQuestBox = Instantiate(subQuestPrefab, questPrefabParent);
            var currentSubQuest = SelectSubQuest(sideQuest);
            if(sideQuestBox.TryGetComponent<QuestPage>(out var page))
            {
                page.Set(sideQuest.text,currentSubQuest == null ? "" : currentSubQuest.text,sideQuest, () =>
                {
                    SetPageContents(sideQuest.text,currentSubQuest == null ? "This Quest Does't Have sub quests" : currentSubQuest.text,sideQuest.description);                    
                    currentQuest = sideQuest;
                    PopulateRewards();
                    navigateButton.SetActive(true);
                });
            }
            subQuestBox.Add(sideQuestBox);
        }
    }
    public void PopulateRewards()
    {
        if(rewardsParent.childCount >0)
        {
            for (int i = 0; i < rewardsParent.childCount; i++)
            {
                Destroy(rewardsParent.GetChild(i));
            }
        }
        if(currentQuest == null) return;

        if(currentQuest.questRewards.Count>0)
        {
            foreach (var reward in currentQuest.questRewards)
            {
                GameObject rewardUI = Instantiate(rewardPrefab, rewardsParent);
                rewardUI.GetComponentInChildren<Image>().sprite = reward.rewardIcon;
            }
        }
        noRewardsText.SetActive(currentQuest.questRewards.Count>0);
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
        for (int i = 0; i < mainQuestsBox.Count; i++)
        {
            GameObject mainQuestUI = mainQuestsBox[i];
            Quest mainQuest = QuestSystem.instance.GetQuest(mainQuestUI.GetComponent<QuestPage>().quest.text);
            mainQuestUI.SetActive(!mainQuest.isDone && isMain && i == QuestSystem.instance.GetCurrentQuestIndex());
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
    public void NavigateToQuest()
    {
        if(currentQuest == null) return;
        if(QuestSystem.instance.quests.Exists(x=>x == currentQuest))
        {
            QuestSystem.instance.AddNewQuest(currentQuest, true, false, 0, false);            
        }
        else
        {
            var currentSubQuest = SelectSubQuest(currentQuest);
            QuestSystem.instance.AddNewQuest(currentSubQuest, false, true, currentQuest.subQuests.FindIndex(x=>x==currentSubQuest),true);            
        }
        QuestPagePanel.SetActive(false);
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
    public void OpenPanel(bool isOpen)
    {
        QuestPagePanel.SetActive(isOpen);
        navigateButton.SetActive(false);
    }
}

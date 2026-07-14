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
        for (int i = questPrefabParent.childCount - 1; i >= 0; i--)
        {
            Destroy(questPrefabParent.GetChild(i).gameObject);
        }
        foreach (var box in mainQuestsBox) Destroy(box);
        foreach (var box in subQuestBox) Destroy(box);
        mainQuestsBox.Clear();
        subQuestBox.Clear();

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
            mainQuestBox.SetActive(true);
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
            sideQuestBox.SetActive(true);
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
        foreach (var mainQuestUI in mainQuestsBox)
        {
            Quest mainQuest = mainQuestUI.GetComponent<QuestPage>().quest;
            mainQuestUI.SetActive(mainQuest != null && !mainQuest.isDone && isMain);
        }
        foreach (var sideQuestUI in subQuestBox)
        {
            Quest sideQuest = sideQuestUI.GetComponent<QuestPage>().quest;
            sideQuestUI.SetActive(sideQuest != null && !sideQuest.isDone && isSide);
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
        if(currentQuest.assignmentManager != null)
        {
            currentQuest.assignmentManager.ActivateQuest();
        }
        QuestPagePanel.SetActive(false);
    }
    public Quest SelectSubQuest(Quest quest)
    {
        if(QuestSystem.instance == null || quest == null) return null;
        foreach (var subQuest in quest.subQuests)
        {
            if (!subQuest.isDone)
            {
                return subQuest;
            }
        }
        return null;
    }
    public void OpenPanel(bool isOpen)
    {
        QuestPagePanel.SetActive(isOpen);
        navigateButton.SetActive(false);
    }
}

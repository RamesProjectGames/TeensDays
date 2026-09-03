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
    List<GameObject> sideQuestBox = new List<GameObject>();
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
        foreach (var box in sideQuestBox) Destroy(box);
        mainQuestsBox.Clear();
        sideQuestBox.Clear();

        for (int i = 0; i < QuestSystem.instance.quests.Count; i++)
        {
            Quest mainQuest = QuestSystem.instance.quests[i];
            if (mainQuest.isDone || !QuestSystem.instance.IsQuestUnlocked(mainQuest))
            {
                continue;
            }
            GameObject mainQuestBox = Instantiate(mainQuestPrefab, questPrefabParent);
            var currentSubQuest = SelectSubQuest(mainQuest);
            if(mainQuestBox.TryGetComponent<QuestPage>(out var page))
            {
                page.Set(mainQuest.text,currentSubQuest == null ? "" : currentSubQuest.text,mainQuest, () =>
                {
                    currentQuest = mainQuest;
                    SetPageContents(mainQuest.text,currentSubQuest == null ? "This Quest Does't Have sub quests" : currentSubQuest.text,mainQuest.description);
                    navigateButton.SetActive(true);
                    PopulateRewards();
                });
            }
            mainQuestBox.SetActive(QuestSystem.instance.GetCurrentQuestIndex() == i);
            mainQuestsBox.Add(mainQuestBox);
        }
        for (int i = 0; i < QuestSystem.instance.sideQuests.Count; i++)
        {
            Quest sideQuest = QuestSystem.instance.sideQuests[i];
            if (sideQuest.isDone || !QuestSystem.instance.IsQuestUnlocked(sideQuest))
            {
                continue;
            }
            GameObject sideQuestBox = Instantiate(subQuestPrefab, questPrefabParent);
            var currentSubQuest = SelectSubQuest(sideQuest);
            if(sideQuestBox.TryGetComponent<QuestPage>(out var page))
            {
                page.Set(sideQuest.text,currentSubQuest == null ? "" : currentSubQuest.text,sideQuest, () =>
                {
                    currentQuest = sideQuest;
                    SetPageContents(sideQuest.text,currentSubQuest == null ? "This Quest Does't Have sub quests" : currentSubQuest.text,sideQuest.description);                    
                    PopulateRewards();
                    navigateButton.SetActive(true);
                });
            }
            sideQuestBox.SetActive(true);
            this.sideQuestBox.Add(sideQuestBox);
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
        noRewardsText.SetActive(currentQuest.questRewards.Count<=0);
    }
    public void SetPageContents(string title, string subTitle, string desc)
    {
        questTitle.text = title;
        subQuestTitle.text = subTitle;
        questDescription.text = desc;
        if(CheckOnGoingQuest())
        {
            navigateButton.GetComponentInChildren<TMP_Text>().text = "Cancel Navigation";
        }
        else
        {
            navigateButton.GetComponentInChildren<TMP_Text>().text = "Navigate";            
        }
    }
    public void UpdateQuestPage(bool isMain = true, bool isSide = true)
    {
        if(QuestSystem.instance == null) return;
        for (int i = 0; i < mainQuestsBox.Count; i++)
        {
            GameObject mainQuestUI = mainQuestsBox[i];
            Quest mainQuest = mainQuestUI.GetComponent<QuestPage>().quest;
            mainQuestUI.SetActive(mainQuest != null && QuestSystem.instance.GetCurrentQuestIndex() == i && !mainQuest.isDone && isMain);
        }
        for (int i = 0; i < sideQuestBox.Count; i++)
        {
            GameObject sideQuestUI = sideQuestBox[i];
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
        if(currentQuest == null || !QuestSystem.instance.IsQuestUnlocked(currentQuest)) return;
        if(CheckOnGoingQuest())
        {
            if(QuestSystem.instance.quests.Exists(x => x == currentQuest))
            {
                QuestSystem.instance.RemoveExistingQuest(currentQuest, QuestSystem.instance.quests.Exists(x => x == currentQuest));
            }
            CancelNavigation();
        }
        else
        {
            if (QuestSystem.instance.quests.Exists(x => x == currentQuest))
            {
                QuestSystem.instance.AddNewQuest(currentQuest, true, false, 0, false);
            }
            else
            {
                var currentSubQuest = SelectSubQuest(currentQuest);
                int currentSubQuestIndex = currentQuest.subQuests.FindIndex(x => x == currentSubQuest) < 0 ? 0 : currentQuest.subQuests.FindIndex(x => x == currentSubQuest);
                QuestSystem.instance.AddNewQuest(currentQuest, false, true, currentSubQuestIndex, true);
                QuestSystem.instance.SetCurrentSideQuestIndex(QuestSystem.instance.sideQuests.FindIndex(x => x == currentQuest));
                QuestSystem.instance.SetCurrentSideSubQuestIndex(currentSubQuestIndex);
            }
            TeleportController.Instance.TeleportTo(currentQuest.targetTransform);
            if (currentQuest.assignmentManager != null)
            {
                currentQuest.assignmentManager.ActivateQuest();
            }
            QuestPagePanel.SetActive(false);
        }
    }
    public void CancelNavigation()
    {
        QuestSystem.instance.CancelNavigation();
        if (currentQuest.assignmentManager != null)
        {
            currentQuest.assignmentManager.DeactivateQuest();
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
    public bool CheckOnGoingQuest()
    {
        bool isMain = QuestSystem.instance.quests.Exists(x=>x==currentQuest);
        bool isOngoing = false;
        if(isMain)
        {
            isOngoing = QuestSystem.instance.GetCurrentQuestIndex() == QuestSystem.instance.quests.FindIndex(x=>x==currentQuest);
        }
        else
        {
            isOngoing = QuestSystem.instance.GetCurrentSideQuestIndex() == QuestSystem.instance.sideQuests.FindIndex(x=>x==currentQuest);
        }
        return isOngoing;
    }
}

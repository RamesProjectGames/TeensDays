using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroceriesManager : AssignmentManager
{
    public static GroceriesManager Instance;
    public GameObject NPCRelated;
    public List<GameObject> groceriesLocations = new List<GameObject>();
    public string questName;
    public string inCompleteDialogue;
    public string completedDialogue;
    public int rewardAmount;
    public int repeatableRewradAmount;
    int currentGrocery;
    InteractableNPC interactable;
    void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        if(NPCRelated.TryGetComponent<InteractableNPC>(out var interactable))
        {
            this.interactable = interactable;
        }
        NPCRelated.SetActive(false);
        currentGrocery = 0;
        for (int i = 0; i < groceriesLocations.Count; i++)
        {
            groceriesLocations[i].SetActive(false);
        }

    }
    public override void ActivateQuest()
    {
        base.ActivateQuest();
        LoadProgressFromQuestState(questName, true);
        NPCRelated.SetActive(true);
        currentGrocery = 0;
        if(interactable !=null)
        {
            interactable.npcId = inCompleteDialogue;
            interactable.onTalkEnded.RemoveAllListeners();
            interactable.OnTalkStart.RemoveAllListeners();
            interactable.OnTalkStart.AddListener(StartQuest);
        }
    }
    public override void DeactivateQuest()
    {
        base.DeactivateQuest();
        NPCRelated.SetActive(false);
        currentGrocery = 0;
        if(interactable !=null)
        {
            interactable.npcId = inCompleteDialogue;
            interactable.onTalkEnded.RemoveAllListeners();
            interactable.OnTalkStart.RemoveAllListeners();
            interactable.OnTalkStart.AddListener(StartQuest);
        }
    }
    public void StartQuest()
    {
        MarkStarted();
        SetProgress(0f);
        NPCRelated.SetActive(false);
        currentGrocery = 0;
        if(interactable !=null)
        {
            interactable.npcId = completedDialogue;
            interactable.onTalkEnded.RemoveAllListeners();
        }
        string listOfGroceries = "Barang Belanja : \n";
        for (int i = 0; i < groceriesLocations.Count; i++)
        {
            groceriesLocations[i].SetActive(i == currentGrocery);
            if(i> currentGrocery)
            {
                listOfGroceries += $"{groceriesLocations[i].name} \n";
            }
            else
            {
                listOfGroceries += $"<s>{groceriesLocations[i].name}<s> \n";
            }
        }
        // QuestSystem.instance.AddNewQuest(QuestSystem.instance.GetQuest(questName,true),false,true,0,true);
        QuestSystem.instance.UpdateCurrentQuestInfo(QuestSystem.instance.GetQuest(questName,true),false,listOfGroceries);
    }
    public void ProgressQuest()
    {
        if (currentGrocery >= groceriesLocations.Count)
        {
            FinishQuest();
            return;
        }

        currentGrocery += 1;
        if (currentGrocery < groceriesLocations.Count)
        {
            QuestPathManager.Instance.SetQuestTarget(groceriesLocations[currentGrocery].transform);
        }

        if (groceriesLocations.Count > 0)
        {
            SetProgress(Mathf.Clamp01((float)currentGrocery / groceriesLocations.Count));
        }

        if (currentGrocery >= groceriesLocations.Count)
        {
            FinishQuest();
            return;
        }

        string listOfGroceries = "Barang Belanja : \n";
        for (int i = 0; i < groceriesLocations.Count; i++)
        {
            groceriesLocations[i].SetActive(i== currentGrocery);
            if(i> currentGrocery)
            {
                listOfGroceries += $"{groceriesLocations[i].name} \n";
            }
            else
            {
                listOfGroceries += $"<s>{groceriesLocations[i].name}<s> \n";
            }
        }
        var questRelated = QuestSystem.instance.GetQuest(questName,true);
        if(questRelated != null)
        {
            QuestSystem.instance.UpdateCurrentQuestInfo(questRelated,false,listOfGroceries);
        }
    }
    public void FinishQuest()
    {
        NPCRelated.SetActive(true);
        if(interactable !=null)
        {
            interactable.npcId = completedDialogue;
            interactable.onTalkEnded.AddListener(() =>
            {
                QuestSystem.instance.MarkQuestDone(3, 1, true, true);
                TrackProgressFromSubQuests(questName, true);
                var questRelated = QuestSystem.instance.GetQuest(questName, true);
                if (questRelated != null)
                {
                    if (!questRelated.isDone)
                    {
                        GameManager.Instance.playerData.currMoney += rewardAmount;
                    }
                    else
                    {
                        GameManager.Instance.playerData.currMoney += repeatableRewradAmount;
                    }

                    if (questRelated.isDone)
                    {
                        CompleteProgress();
                    }
                }
                ResetQuest();
            });
        }        
        QuestSystem.instance.MarkQuestDone(3, 0, true, true);
        TrackProgressFromSubQuests(questName, true);
        QuestSystem.instance.AddNewQuest(QuestSystem.instance.GetQuest(questName,true),false,true,1,true);
        
    }
    public void ResetQuest()
    {
        if(interactable != null)
        {
            interactable.npcId = inCompleteDialogue;
            interactable.onTalkEnded.RemoveAllListeners();
            interactable.OnTalkStart.RemoveAllListeners();
            interactable.OnTalkStart.AddListener(StartQuest);
        }
    }
}

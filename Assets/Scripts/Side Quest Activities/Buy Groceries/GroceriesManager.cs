using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroceriesManager : AssignmentManager
{
    public static GroceriesManager Instance;
    public GameObject NPCRelated;
    public GameObject groceryLocation;
    public List<GameObject> groceriesLocations = new List<GameObject>();
    [SerializeField] private float groceryLocationArrivalDistance = 3f;
    public string questName;
    public string inCompleteDialogue;
    public string completedDialogue;
    public int rewardAmount;
    public int repeatableRewradAmount;
    public int currentGrocery;
    InteractableNPC interactable;
    private bool hasReachedGroceryLocation;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (!isStarted || hasReachedGroceryLocation || groceryLocation == null)
        {
            return;
        }

        var playerInteraction = FindAnyObjectByType<PlayerInteraction>();
        if (playerInteraction == null || playerInteraction.playerTransform == null)
        {
            return;
        }

        if (Vector3.Distance(playerInteraction.playerTransform.position, groceryLocation.transform.position) <= groceryLocationArrivalDistance)
        {
            hasReachedGroceryLocation = true;
            UpdatePathTarget();
        }
    }

    void Start()
    {
        if(NPCRelated.TryGetComponent<InteractableNPC>(out var interactable))
        {
            this.interactable = interactable;
        }
        NPCRelated.transform.parent.gameObject.SetActive(false);
        currentGrocery = 0;
        for (int i = 0; i < groceriesLocations.Count; i++)
        {
            groceriesLocations[i].SetActive(false);
        }

    }
    private void RestoreCurrentGroceryProgress()
    {
        currentGrocery = 0;
        hasReachedGroceryLocation = false;
        var questRelated = QuestSystem.instance != null ? QuestSystem.instance.GetQuest(questName, true) : null;
        if (questRelated != null)
        {
            for (int i = 0; i < questRelated.subQuests.Count && i < groceriesLocations.Count; i++)
            {
                if (questRelated.subQuests[i].isDone)
                {
                    currentGrocery = i + 1;
                    hasReachedGroceryLocation = true;
                }
            }
        }

        if (currentGrocery < 0)
        {
            currentGrocery = 0;
        }

        if (groceriesLocations.Count > 0)
        {
            for (int i = 0; i < groceriesLocations.Count; i++)
            {
                groceriesLocations[i].SetActive(i == currentGrocery);
            }
        }

        UpdateShopListInfo();
        UpdatePathTarget();
    }

    private void UpdateShopListInfo()
    {
        string listOfGroceries = "Barang Belanja : \n";
        for (int i = 0; i < groceriesLocations.Count; i++)
        {
            var groceryName = groceriesLocations[i] != null ? groceriesLocations[i].name : $"Item {i + 1}";
            groceriesLocations[i].SetActive(i == currentGrocery);

            if (i < currentGrocery)
            {
                listOfGroceries += $"<color=#4CAF50><s>{groceryName}</s></color> \n";
            }
            else if (i == currentGrocery)
            {
                listOfGroceries += $"<color=#FFB300>{groceryName}</color> \n";
            }
            else
            {
                listOfGroceries += $"<color=#FFFFFF>{groceryName}</color> \n";
            }
        }

        var questRelated = QuestSystem.instance != null ? QuestSystem.instance.GetQuest(questName, true) : null;
        if (questRelated != null)
        {
            QuestSystem.instance.UpdateCurrentQuestInfo(questRelated, false, listOfGroceries);
        }
    }

    private void UpdatePathTarget()
    {
        if (QuestPathManager.Instance == null)
        {
            return;
        }

        if (!hasReachedGroceryLocation && groceryLocation != null)
        {
            QuestPathManager.Instance.SetQuestTarget(groceryLocation.transform);
        }
        else if (currentGrocery < groceriesLocations.Count && groceriesLocations[currentGrocery] != null)
        {
            QuestPathManager.Instance.SetQuestTarget(groceriesLocations[currentGrocery].transform);
        }
        else if (groceryLocation != null)
        {
            QuestPathManager.Instance.SetQuestTarget(groceryLocation.transform);
        }
    }

    public override void ActivateQuest()
    {
        base.ActivateQuest();
        LoadProgressFromQuestState(questName, true);
        RestoreCurrentGroceryProgress();
        NPCRelated.transform.parent.gameObject.SetActive(true);
        if(interactable !=null)
        {
            interactable.npcId = inCompleteDialogue;
            interactable.onTalkEnded.RemoveAllListeners();
            interactable.OnTalkStart.RemoveAllListeners();
            interactable.onTalkEnded.AddListener(StartQuest);
        }
    }
    public override void DeactivateQuest()
    {
        base.DeactivateQuest();
        NPCRelated.transform.parent.gameObject.SetActive(false);
        currentGrocery = 0;
        if(interactable !=null)
        {
            interactable.npcId = inCompleteDialogue;
            interactable.onTalkEnded.RemoveAllListeners();
            interactable.OnTalkStart.RemoveAllListeners();
            interactable.onTalkEnded.AddListener(StartQuest);
        }
    }
    public void StartQuest()
    {
        MarkStarted();
        SetProgress(0f);
        NPCRelated.transform.parent.gameObject.SetActive(false);
        currentGrocery = 0;
        hasReachedGroceryLocation = false;
        if(interactable !=null)
        {
            interactable.npcId = completedDialogue;
            interactable.onTalkEnded.RemoveAllListeners();
        }
        UpdateShopListInfo();
        UpdatePathTarget();
    }
    public void ProgressQuest()
    {
        if (currentGrocery >= groceriesLocations.Count)
        {
            FinishQuest();
            return;
        }

        if (currentGrocery >= groceriesLocations.Count - 1)
        {
            currentGrocery = groceriesLocations.Count;
            SetProgress(1f);
            UpdateShopListInfo();
            var questRelated = QuestSystem.instance != null ? QuestSystem.instance.GetQuest(questName, true) : null;
            if (questRelated != null)
            {
                QuestSystem.instance.UpdateCurrentQuestInfo(questRelated, false, "Barang Belanja : \n<color=#4CAF50><s>Semua Barang Sudah Dibeli \nLapor ke Ibu</s></color>");
            }
            FinishQuest();
            return;
        }

        currentGrocery += 1;
        UpdatePathTarget();

        if (groceriesLocations.Count > 0)
        {
            SetProgress(Mathf.Clamp01((float)currentGrocery / groceriesLocations.Count));
        }

        UpdateShopListInfo();
    }
    public void FinishQuest()
    {
        NPCRelated.transform.parent.gameObject.SetActive(true);
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
            NPCRelated.gameObject.SetActive(false);
            interactable.npcId = inCompleteDialogue;
            interactable.onTalkEnded.RemoveAllListeners();
            interactable.OnTalkStart.RemoveAllListeners();
            interactable.onTalkEnded.AddListener(StartQuest);
        }
    }
}

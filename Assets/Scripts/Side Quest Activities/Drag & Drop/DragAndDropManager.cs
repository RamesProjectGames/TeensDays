using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class DragAndDropManager : AssignmentManager
{
    public static DragAndDropManager Instance;

    [Header("Quest Related Variables")]
    public string questName;    
    public string inCompleteDialogue;
    public string completedDialogue;
    [SerializeField] private GameObject puzzlePanel;
    public InteractableNPC RelatedNPC;
    [Header("Prefabs")]
    [SerializeField] private DragSlot slotPrefab;
    [SerializeField] private DragItem itemPrefab;

    [Header("Parents")]
    [SerializeField] private RectTransform slotContainer;
    [SerializeField] private RectTransform itemContainer;

    [Header("Settings")]
    [SerializeField] private int itemCount = 12;
    [SerializeField] private Vector2 startPosition;
    [SerializeField] private Vector2 itemSize = new Vector2(80, 200);
    [SerializeField] private float gap = 10f;
    [Header("Events")]
    public UnityEvent onStartPuzzle;
    public UnityEvent onPuzzleSolved;

    private readonly List<DragSlot> slots = new();
    private readonly List<DragItem> items = new();

    private void Awake()
    {
        Instance = this;
    }

    public override void ActivateQuest()
    {
        base.ActivateQuest();
        RelatedNPC.SetNewDialogue(inCompleteDialogue);
        RelatedNPC.onTalkEnded.RemoveAllListeners();
        RelatedNPC.onTalkEnded.AddListener(StartPuzzle);
        var questRelated = QuestSystem.instance.GetQuest(questName,true);
        if(questRelated != null)
        {
            QuestSystem.instance.UpdateCurrentQuestInfo(questRelated, false, $"Bantu Ibu Kartika untuk menyusun buku-buku di rak dengan urutan yang benar");            
        }
    }
    public override void DeactivateQuest()
    {
        base.DeactivateQuest();
        puzzlePanel.SetActive(false);
        Clear();
    }
#if UNITY_EDITOR
    private void OnValidate()
    {
        UpdateLayout();
    }
#endif

    private void UpdateLayout()
    {
        if (slots == null || items == null)
            return;

        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i] == null)
                continue;

            slots[i].RectTransform.anchoredPosition =
                startPosition + new Vector2((itemSize.x + gap) * i, 0);
        }

        foreach (DragItem item in items)
        {
            if (item == null || item.CurrentSlot == null)
                continue;

            item.RectTransform.anchoredPosition =
                item.CurrentSlot.RectTransform.anchoredPosition;
        }
    }
    public void StartPuzzle()
    {
        puzzlePanel.SetActive(true);
        GeneratePuzzle();
        int questIndex = QuestSystem.instance.GetQuestIndex(questName,true);
        int subQuestIndex = QuestSystem.instance.GetSubQuestIndex(questName,completedDialogue,true);
        if(questIndex != -1 && subQuestIndex != -1)
        {
            QuestSystem.instance.MarkQuestDone(questIndex, subQuestIndex, true, true);
        }
    }
    public void GeneratePuzzle()
    {
        Clear();

        CreateSlots();
        CreateItems();
        ShuffleItems();
        
        onStartPuzzle?.Invoke();
    }

    void Clear()
    {
        foreach (Transform child in slotContainer)
            Destroy(child.gameObject);

        foreach (Transform child in itemContainer)
            Destroy(child.gameObject);

        slots.Clear();
        items.Clear();
    }

    void CreateSlots()
    {
        for (int i = 0; i < itemCount; i++)
        {
            DragSlot slot = Instantiate(slotPrefab, slotContainer);

            RectTransform rt = slot.GetComponent<RectTransform>();

            rt.anchoredPosition = startPosition + new Vector2((itemSize.x + gap) * i, 0);

            slot.SlotIndex = i;

            slots.Add(slot);
        }
    }

    void CreateItems()
    {
        for (int i = 0; i < itemCount; i++)
        {
            DragItem item = Instantiate(itemPrefab, itemContainer);

            item.SetNumber(i + 1);

            items.Add(item);
        }
    }

    void ShuffleItems()
    {
        List<int> indexes = new();

        for (int i = 0; i < itemCount; i++)
            indexes.Add(i);

        for (int i = 0; i < indexes.Count; i++)
        {
            int r = Random.Range(i, indexes.Count);

            (indexes[i], indexes[r]) = (indexes[r], indexes[i]);
        }

        for (int i = 0; i < itemCount; i++)
        {
            DragItem item = items[indexes[i]];
            DragSlot slot = slots[i];

            item.CurrentSlot = slot;
            slot.CurrentItem = item;

            item.transform.position = slot.transform.position;
        }
    }

    public void CheckSolved()
    {
        foreach (DragSlot slot in slots)
        {
            if (slot.CurrentItem == null)
                return;

            if (slot.CurrentItem.CorrectOrder != slot.SlotIndex + 1)
                return;
        }

        RelatedNPC.SetNewDialogue(completedDialogue);
        RelatedNPC.onTalkEnded.RemoveAllListeners();
        puzzlePanel.SetActive(false);

        
        int questIndex = QuestSystem.instance.GetQuestIndex(questName,true);
        int subQuestIndex = QuestSystem.instance.GetSubQuestIndex(questName,completedDialogue,true);
        if(questIndex != -1 && subQuestIndex != -1)
        {
            QuestSystem.instance.MarkQuestDone(questIndex, subQuestIndex, true, true);
        }

        var questRelated = QuestSystem.instance.GetQuest(questName,true);
        if(questRelated != null)
        {
            if(!questRelated.isDone)
            {
                foreach (var reward in questRelated.questRewards)
                {
                    if(reward.type == QuestRewardType.Money)
                    {
                        GameManager.Instance.playerData.currMoney += reward.rewardAmount;
                    }
                    else if(reward.type == QuestRewardType.Diamonds)
                    {
                        GameManager.Instance.playerData.currDiamond += reward.rewardAmount;
                    }
                }
            }
            else
            {
                foreach (var reward in questRelated.questRewards)
                {
                    if(reward.type == QuestRewardType.Money)
                    {
                        GameManager.Instance.playerData.currMoney += reward.rewardAmount / 10;
                    }
                    else if(reward.type == QuestRewardType.Diamonds)
                    {
                        GameManager.Instance.playerData.currDiamond += reward.rewardAmount /10;
                    }
                }
            }
        }
        onPuzzleSolved?.Invoke();
    }

    public DragSlot GetNearestSlot(Vector2 position)
    {
        float closest = float.MaxValue;
        DragSlot nearest = null;

        foreach (DragSlot slot in slots)
        {
            float dist = Vector2.Distance(position, slot.transform.position);

            if (dist < closest)
            {
                closest = dist;
                nearest = slot;
            }
        }

        return nearest;
    }
}
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class QuestSystem : MonoBehaviour
{
    public static QuestSystem instance;
    public List<Quest> quests = new List<Quest>();
    public List<Quest> sideQuests = new List<Quest>(); // Side quest list
    public List<QuestData> questDatas = new List<QuestData>();
    public List<QuestData> sideQuestDatas = new List<QuestData>(); 
    [SerializeField] private int currentQuestIndex = 0;
    [SerializeField] private int currentSideQuestIndex = 0;
    [SerializeField] private float blinkSpeed = 2f;

    public QuestUIManager questUIManager;
    public QuestPathManager questPathManager;

    private void Awake()
    {
        instance = this;

    }

    private void Start()
    {
        InitilializeQuestData();
    }

    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.K))
        //{
        //    // contoh: set parent0 sub0 done
        //    MarkQuestDone(0, 0, true, false);
        //    Debug.Log("Test masuk");
        //}

        //UpdateQuestDisplay();
        //CheckAutoCompleteQuests();
        UpdateQuestOutlines();
    }
    public void InitilializeQuestData()
    {
        foreach (var quest in quests)
        {
            if(!questDatas.Exists(qd => string.Compare(qd.questName, quest.text, System.StringComparison.OrdinalIgnoreCase) == 0))
            {
                QuestData questData = new QuestData
                {
                    questName = quest.text,
                    isDone = quest.isDone
                };
                foreach (var subquest in quest.subQuests)
                {
                    QuestData subQuestData = new QuestData
                    {
                        questName = subquest.text,
                        isDone = subquest.isDone
                    };
                    questData.subQuests.Add(subQuestData);                
                }
                questDatas.Add(questData);
            }
        }
        foreach (var sideQuest in sideQuests)
        {
            if(!sideQuestDatas.Exists(qd => string.Compare(qd.questName, sideQuest.text, System.StringComparison.OrdinalIgnoreCase) == 0))
            {
                QuestData sideQuestData = new QuestData
                {
                    questName = sideQuest.text,
                    isDone = sideQuest.isDone
                };
                foreach (var subquest in sideQuest.subQuests)
                {
                    QuestData subQuestData = new QuestData
                    {
                        questName = subquest.text,
                        isDone = subquest.isDone
                    };
                    sideQuestData.subQuests.Add(subQuestData);
                }
                sideQuestDatas.Add(sideQuestData);
            }
        }
        foreach (var mainQuest in GameManager.Instance.playerData.mainQuests.list)
        {
            if (questDatas.Exists(qd => string.Compare(qd.questName, mainQuest.questName, System.StringComparison.OrdinalIgnoreCase) == 0))
            {
                int mainQuestIndex = questDatas.FindIndex(qd => string.Compare(qd.questName, mainQuest.questName, System.StringComparison.OrdinalIgnoreCase) == 0);
                questDatas[mainQuestIndex].isDone = mainQuest.isDone;
                foreach (var subQuest in mainQuest.subQuests)
                {
                    int subQuestIndex = questDatas[mainQuestIndex].subQuests.FindIndex(sq => string.Compare(sq.questName, subQuest.questName, System.StringComparison.OrdinalIgnoreCase) == 0);
                    questDatas[mainQuestIndex].subQuests[subQuestIndex].isDone = subQuest.isDone;
                }
            }
        }
        foreach (var sideQuest in GameManager.Instance.playerData.sideQuests.list)
        {
            if (sideQuestDatas.Exists(qd => string.Compare(qd.questName, sideQuest.questName, System.StringComparison.OrdinalIgnoreCase) == 0))
            {
                int sideQuestIndex = sideQuestDatas.FindIndex(sq => string.Compare(sq.questName, sideQuest.questName, System.StringComparison.OrdinalIgnoreCase) == 0);
                sideQuestDatas[sideQuestIndex].isDone = sideQuest.isDone;  
                foreach (var subQuest in sideQuest.subQuests)
                {
                    int subQuestIndex = sideQuestDatas[sideQuestIndex].subQuests.FindIndex(sq => string.Compare(sq.questName, subQuest.questName, System.StringComparison.OrdinalIgnoreCase) == 0);
                    sideQuestDatas[sideQuestIndex].subQuests[subQuestIndex].isDone = subQuest.isDone;
                }
            }
        }
    }
    public void LoadQuests()
    {
        foreach (var mainQuest in GameManager.Instance.playerData.mainQuests.list)
        {
            if (questDatas.Exists(qd => string.Compare(qd.questName, mainQuest.questName, System.StringComparison.OrdinalIgnoreCase) == 0))
            {
                int mainQuestIndex = questDatas.FindIndex(qd => string.Compare(qd.questName, mainQuest.questName, System.StringComparison.OrdinalIgnoreCase) == 0);
                questDatas[mainQuestIndex].isDone = mainQuest.isDone;
                foreach (var subQuest in mainQuest.subQuests)
                {
                    int subQuestIndex = questDatas[mainQuestIndex].subQuests.FindIndex(sq => string.Compare(sq.questName, subQuest.questName, System.StringComparison.OrdinalIgnoreCase) == 0);
                    questDatas[mainQuestIndex].subQuests[subQuestIndex].isDone = subQuest.isDone;
                }
            }
            if (quests.Exists(qd => string.Compare(qd.text, mainQuest.questName, System.StringComparison.OrdinalIgnoreCase) == 0))
            {
                int mainQuestIndex = quests.FindIndex(sq => string.Compare(sq.text, mainQuest.questName, System.StringComparison.OrdinalIgnoreCase) == 0);
                quests[mainQuestIndex].isDone = mainQuest.isDone;
                foreach (var subQuest in mainQuest.subQuests)
                {
                    int subQuestIndex = quests[mainQuestIndex].subQuests.FindIndex(sq => string.Compare(sq.text, subQuest.questName, System.StringComparison.OrdinalIgnoreCase) == 0);
                    quests[mainQuestIndex].subQuests[subQuestIndex].isDone = subQuest.isDone;
                }
            }
        }
        foreach (var sideQuest in GameManager.Instance.playerData.sideQuests.list)
        {
            if (sideQuestDatas.Exists(qd => string.Compare(qd.questName, sideQuest.questName, System.StringComparison.OrdinalIgnoreCase) == 0))
            {
                int sideQuestIndex = sideQuestDatas.FindIndex(sq => string.Compare(sq.questName, sideQuest.questName, System.StringComparison.OrdinalIgnoreCase) == 0);
                sideQuestDatas[sideQuestIndex].isDone = sideQuest.isDone;  
                foreach (var subQuest in sideQuest.subQuests)
                {
                    int subQuestIndex = sideQuestDatas[sideQuestIndex].subQuests.FindIndex(sq => string.Compare(sq.questName, subQuest.questName, System.StringComparison.OrdinalIgnoreCase) == 0);
                    sideQuestDatas[sideQuestIndex].subQuests[subQuestIndex].isDone = subQuest.isDone;
                }
            }
            if (sideQuests.Exists(qd => string.Compare(qd.text, sideQuest.questName, System.StringComparison.OrdinalIgnoreCase) == 0))
            {
                int sideQuestIndex = sideQuests.FindIndex(sq => string.Compare(sq.text, sideQuest.questName, System.StringComparison.OrdinalIgnoreCase) == 0);
                sideQuests[sideQuestIndex].isDone = sideQuest.isDone;
                foreach (var subQuest in sideQuest.subQuests)
                {
                    int subQuestIndex = sideQuests[sideQuestIndex].subQuests.FindIndex(sq => string.Compare(sq.text, subQuest.questName, System.StringComparison.OrdinalIgnoreCase) == 0);
                    sideQuests[sideQuestIndex].subQuests[subQuestIndex].isDone = subQuest.isDone;
                }
            }
        }
    }
    public void SetCurrentQuestIndex(int index)
    {
        currentQuestIndex = index;
        GameManager.Instance.playerData.questIndex = index; // Sync ke playerData
        ActivateQuestObject(currentQuestIndex);
        UpdateNPCs();
    }
    public void SetCurrentSideQuestIndex(int index)
    {
        currentSideQuestIndex = index;
        GameManager.Instance.playerData.sideQuestIndex = index; // Sync ke playerData
        ActivateQuestObject(currentSideQuestIndex);
    }
    public int GetCurrentQuestIndex()
    {
        return currentQuestIndex;
    }
    public int GetCurrentSideQuestIndex()
    {
        return currentSideQuestIndex;
    }

    public bool HasQuest(Quest questData)
    {
        return quests.Contains(questData);
    }

    public void UpdateQuestDisplay()
    {
        foreach (var quest in quests)
        {
            UpdateSingleQuestDisplay(quest);
        }
    }

    public void UpdateSingleQuestDisplay(Quest quest)
    {
        if (quest.questText == null) return;
        quest.questText.text = quest.isDone ? $"<s>{quest.text}</s>" : quest.text;

        // Update semua subquest kalau ada
        foreach (var sub in quest.subQuests)
        {
            if (sub.questText == null) continue;
            sub.questText.text = sub.isDone ? $"<s>{sub.text}</s>" : sub.text;
        }
    }

    public void MarkQuestDone(int parentIndex, int questIndex, bool isSubQuest, bool isSideQuest = false)
    {
        List<Quest> questList = isSideQuest ? sideQuests : quests;
        List<QuestData> questDataList = isSideQuest ? sideQuestDatas : questDatas;
        if (isSubQuest)
        {
            if(parentIndex >= 0 && parentIndex < questDataList.Count)
            {
                QuestData subQuestData = questDataList[parentIndex].subQuests[questIndex];
                subQuestData.isDone = true;
                bool allDone = questDataList[parentIndex].subQuests.All(sq => sq.isDone);
                if(allDone)
                {
                    questDataList[parentIndex].isDone = true;
                    GameManager.Instance.SavePlayerDataToCloud();
                }          
            }
            if (parentIndex >= 0 && parentIndex < questList.Count)
            {
                Quest subQuest = questList[parentIndex].subQuests[questIndex];
                subQuest.isDone = true;
                UpdateSingleQuestDisplay(subQuest);

                bool allDone = questList[parentIndex].subQuests.All(sq => sq.isDone);
                if (allDone)
                {
                    questList[parentIndex].isDone = true;
                    UpdateSingleQuestDisplay(questList[parentIndex]);
                }
            }            
        }
        else
        {
            if(parentIndex >= 0 && parentIndex < questDataList.Count)
            {
                questDataList[questIndex].isDone = true;
                GameManager.Instance.SavePlayerDataToCloud();
                
            }
            if (questIndex >= 0 && questIndex < questList.Count)
            {
                questList[questIndex].isDone = true;
                UpdateSingleQuestDisplay(questList[questIndex]);
            }
        }
    }

    public void CheckAutoCompleteQuests()
    {
        if (currentQuestIndex < quests.Count)
        {
            var quest = quests[currentQuestIndex];
            Debug.Log($"[CheckAutoComplete] Cek quest {currentQuestIndex}: {quest.text}, isDone={quest.isDone}");

            foreach (var sq in quest.subQuests)
            {
                Debug.Log($"   Subquest '{sq.text}' -> isDone={sq.isDone}");
            }

             if (quest.isDone && quest.subQuests.All(sq => sq.isDone))
             {
                 Debug.Log("✅ Semua subquest selesai, quest utama done!");
                 quest.isDone = true;
                 GameManager.Instance.playerData.expLevel += quest.expForQuest;
                 currentQuestIndex++;
                 ActivateQuestObject(currentQuestIndex);
                 UpdateNPCs();
             }
        }
        else
        {
            Debug.Log($"[CheckAutoComplete] currentQuestIndex {currentQuestIndex} out of range (total={quests.Count})");
        }

        if (currentSideQuestIndex < sideQuests.Count)
        {
            var quest = sideQuests[currentSideQuestIndex];

            // Hanya cek side quest yang belum selesai
             if (quest.isDone && quest.subQuests.All(sq => sq.isDone))
             {
                 Debug.Log($"✅ Semua subquest side quest '{quest.text}' selesai!");
                 quest.isDone = true;
                 GameManager.Instance.playerData.expLevel += quest.expForQuest;
                 currentSideQuestIndex++;
                 ActivateQuestObject(currentSideQuestIndex);
             }

        }


    }
    public void UpdateNPCs()
    {
        for (int i = 0; i < quests.Count; i++)
        {
            if (quests[i].npcObject == null) continue;

            // NPC aktif hanya untuk quest yang sedang aktif
            bool isActiveQuest = (i == currentQuestIndex);

            quests[i].npcObject.SetActive(isActiveQuest);
        }
    }

    public void ActivateQuestObject(int index)
    {
        for (int i = 0; i < quests.Count; i++)
        {
            if (quests[i].questUIObject != null)
                quests[i].questUIObject.SetActive(i == index);
        }

        for (int i = 0; i < sideQuests.Count; i++)
        {
            if (sideQuests[i].questUIObject != null)
                sideQuests[i].questUIObject.SetActive(i == index);
        }
    }

    private void UpdateQuestOutlines()
    {
        for (int i = 0; i < quests.Count; i++)
        {
            var outline = quests[i].questOutline;
            if (outline == null) continue;

            if (i == currentQuestIndex)
            {
                outline.enabled = true;

                float alpha = Mathf.PingPong(Time.time * blinkSpeed, 1f);

                Color c = outline.OutlineColor;
                c.a = alpha;
                outline.OutlineColor = c;
            }
            else
            {
                outline.enabled = false;
            }
        }
    }

    public void AddNewQuest(Quest questData, bool isMainQuest, bool isSubquest = false, bool isSideQuest = false)
    {
        GameObject newItem = Instantiate(questUIManager.questItemPrefab);

        if (isMainQuest)
        {
            newItem.transform.SetParent(questUIManager.panelMainQuestList, false);

        }
        else
        {
            newItem.transform.SetParent(questUIManager.panelSubQuestList, false);
        }

        TMP_Text questText = newItem.GetComponentInChildren<TMP_Text>();
        if (isMainQuest && !isSubquest)
        {
            questText.color = Color.yellow;
        }
        else if (!isMainQuest && isSubquest)
        {
            Color customBlue;
            if (ColorUtility.TryParseHtmlString("#00EEFF", out customBlue))
            {
                questText.color = customBlue;
            }
        }

        // Judul quest
        TMP_Text mainText = newItem.GetComponentInChildren<TMP_Text>();
        mainText.text = questData.text;

        // Simpan referensi
        questData.questUIObject = newItem;
        questData.questText = mainText;
        questData.questOutline = newItem.GetComponent<Outline>();

        // Spawn subquest
        Transform subQuestParent = newItem.transform.Find("Content");

        if (subQuestParent == null)
        {
            Debug.LogError("Parent untuk subquest tidak ditemukan di prefab! Pastikan ada child bernama Content");
        }

        foreach (var sub in questData.subQuests)
        {
            Debug.Log("Subquest masuk");

            if (sub.questUIObject != null) continue;

            GameObject subItem = Instantiate(questUIManager.subQuestItemPrefab, subQuestParent);
            TMP_Text subText = subItem.GetComponentInChildren<TMP_Text>();
            subText.text = sub.text;

            // simpan referensi subquest → supaya bisa di-update nanti
            sub.questUIObject = subItem;
            sub.questText = subText;
            sub.questOutline = subItem.GetComponent<Outline>();
        }

        if (questData.targetTransform != null)
        {
            questPathManager.SetQuestTarget(questData.targetTransform);
            Debug.Log($"🎯 Quest target diatur ke: {questData.targetTransform.name}");
        }
        else
        {
            Debug.LogWarning($"⚠️ Quest '{questData.text}' tidak memiliki targetTransform!");
        }
    }

}

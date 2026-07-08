using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using TMPro;
using UnityEngine;

public class QuestSystem : MonoBehaviour
{
    public static QuestSystem instance;
    public List<Quest> quests = new List<Quest>();
    public List<Quest> sideQuests = new List<Quest>(); // Side quest list
    public SerializableList<QuestData> main = null;
    public SerializableList<QuestData> side = null;
    [SerializeField] private int currentQuestIndex = 0;
    [SerializeField] private int currentSideQuestIndex = 0;
    [SerializeField] private float blinkSpeed = 2f;
    [SerializeField] private bool enableQuestCheatCodes = true;
    [SerializeField] private KeyCode questCheatSubmitKey = KeyCode.Return;
    [SerializeField] private KeyCode questCheatKeypadSubmitKey = KeyCode.KeypadEnter;

    private string questCheatBuffer = string.Empty;
    private bool questCheatNotReadyWarningShown;

    public QuestUIManager questUIManager;
    public QuestPathManager questPathManager;
    public PlayerInteraction playerInteraction;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        InitilializeQuestData();
        StartCoroutine(LoadQuestsRoutine());
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
        // Initialization: ensure UI reflects current scene quest defaults
        foreach (var q in quests) UpdateSingleQuestDisplay(q);
        foreach (var s in sideQuests) UpdateSingleQuestDisplay(s);
    }
    public void LoadQuests()
    {
        // No-op kept for compatibility. Use LoadQuestsAsync/LoadQuestsRoutine at startup instead.
    }

    private IEnumerator LoadQuestsRoutine()
    {
        var task = LoadQuestsAsync();
        while (!task.IsCompleted)
            yield return null;

        if (task.IsFaulted)
        {
            Debug.LogError($"LoadQuestsAsync failed: {task.Exception}");
        }
        else
        {           
            ApplyLoadedQuests(task.Result);
            if(playerInteraction == null)
            {
                playerInteraction = FindObjectOfType<PlayerInteraction>();
            }
            playerInteraction.StartQuest();
        }
    }

    public async Task<SerializableList<QuestData>> LoadQuestsAsync()
    {
        // Try cloud load first
        try
        {
            string jsonMain = await CloudManager.Instance.LoadFromJSONCloud("mainQuests");
            string jsonSide = await CloudManager.Instance.LoadFromJSONCloud("sideQuests");

            if (!string.IsNullOrEmpty(jsonMain))
            {
                main = JsonUtility.FromJson<SerializableList<QuestData>>(jsonMain);
            }
            if (!string.IsNullOrEmpty(jsonSide))
            {
                side = JsonUtility.FromJson<SerializableList<QuestData>>(jsonSide);
            }

            // Fallback to local cache if cloud data missing
            if (main == null)
            {
                main = LoadLocalQuestCache("mainQuests.json");
            }
            if (side == null)
            {
                side = LoadLocalQuestCache("sideQuests.json");
            }

            // Merge main+side into a single wrapper (we'll pack main into the Result list)
            SerializableList<QuestData> result = new SerializableList<QuestData>();
            if (main != null) result.list.AddRange(main.list);
            if (side != null) result.list.AddRange(side.list);

            return result;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error loading quests from cloud: {e}");
            // fallback local
            var fallback = LoadLocalQuestCache("mainQuests.json") ?? new SerializableList<QuestData>();
            return fallback;
        }
    }

    private SerializableList<QuestData> LoadLocalQuestCache(string fileName)
    {
        try
        {
            string path = System.IO.Path.Combine(Application.persistentDataPath, fileName);
            if (!System.IO.File.Exists(path)) return null;
            string json = System.IO.File.ReadAllText(path);
            if (string.IsNullOrEmpty(json)) return null;
            return JsonUtility.FromJson<SerializableList<QuestData>>(json);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Failed to read local quest cache {fileName}: {e}");
            return null;
        }
    }

    private void ApplyLoadedQuests(SerializableList<QuestData> loaded)
    {
        if (loaded == null || loaded.list == null) return;

        // Apply by matching questName to quests and sideQuests
        foreach (var saved in loaded.list)
        {
            // try main quests
            var main = quests.Find(q => string.Compare(q.text, saved.questName, System.StringComparison.OrdinalIgnoreCase) == 0);
            if (main != null)
            {
                main.isDone = saved.isDone;
                for (int i = 0; i < saved.subQuests.Count && i < main.subQuests.Count; i++)
                {
                    main.subQuests[i].isDone = saved.subQuests[i].isDone;
                }
                UpdateSingleQuestDisplay(main);
                continue;
            }

            // try side quests
            var side = sideQuests.Find(q => string.Compare(q.text, saved.questName, System.StringComparison.OrdinalIgnoreCase) == 0);
            if (side != null)
            {
                side.isDone = saved.isDone;
                for (int i = 0; i < saved.subQuests.Count && i < side.subQuests.Count; i++)
                {
                    side.subQuests[i].isDone = saved.subQuests[i].isDone;
                }
                UpdateSingleQuestDisplay(side);
            }
        }        
        QuestPageManager.Instance.PopulateQuestPage();
    }
    public void SetCurrentQuestIndex(int index)
    {
        currentQuestIndex = index;
        GameManager.Instance.playerData.questIndex = index; // Sync ke playerData
        ActivateQuestObject(currentQuestIndex,true);
        UpdateNPCs();
    }
    public void SetCurrentSideQuestIndex(int index)
    {
        currentSideQuestIndex = index;
        GameManager.Instance.playerData.sideQuestIndex = index; // Sync ke playerData
        ActivateQuestObject(currentSideQuestIndex,false);
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
        if (isSubQuest)
        {
            if (parentIndex >= 0 && parentIndex < questList.Count)
            {
                var parent = questList[parentIndex];
                if (questIndex >= 0 && questIndex < parent.subQuests.Count)
                {
                    parent.subQuests[questIndex].isDone = true;
                    UpdateSingleQuestDisplay(parent.subQuests[questIndex]);
                }

                bool allDone = parent.subQuests.All(sq => sq.isDone);
                if (allDone)
                {
                    parent.isDone = true;
                    UpdateSingleQuestDisplay(parent);
                }

                QuestPageManager.Instance.UpdateQuestPage();
                _ = SaveQuestsAsync();
            }
        }
        else
        {
            if (questIndex >= 0 && questIndex < questList.Count)
            {
                var q = questList[questIndex];
                q.isDone = true;
                UpdateSingleQuestDisplay(q);
                QuestPageManager.Instance.UpdateQuestPage();
                _ = SaveQuestsAsync();
            }
        }
    }
    public Quest GetQuest(string questName, bool isSideQuest = false)
    {
        List<Quest> questList = isSideQuest ? sideQuests : quests;
        Quest quest = questList.Find(x => x.text == questName);
        return quest;
    }
    public Quest GetSubQuest(string parentQuest, string questName, bool isSideQuest)
    {
        List<Quest> questList = isSideQuest ? sideQuests : quests;
        Quest quest = questList.Find(x => x.text == parentQuest);
        Quest subQuest = quest.subQuests.Find(x => x.text == questName);
        return subQuest;
    }
    public int GetQuestIndex(string questName, bool isSideQuest = false)
    {
        List<Quest> questList = isSideQuest ? sideQuests : quests;
        int quest = questList.FindIndex(x => x.text == questName);
        return quest;
    }
    public int GetSubQuestIndex(string parentQuest, string questName, bool isSideQuest)
    {
        List<Quest> questList = isSideQuest ? sideQuests : quests;
        Quest quest = questList.Find(x => x.text == parentQuest);
        int subQuest = quest.subQuests.FindIndex(x => x.text == questName);
        return subQuest;
    }
    [ContextMenu("Test Mark Main Quest 0 Subquest 0 Done")]
    public void SaveQuests()
    {
        _ = SaveQuestsAsync();
    }
    [ContextMenu("Test Complete All Quests")]
    public void CompleteAllQuestsForTesting()
    {
        foreach (var q in quests)
        {
            q.isDone = true;
            foreach (var sub in q.subQuests)
            {
                sub.isDone = true;
            }
        }
        foreach (var s in sideQuests)
        {
            s.isDone = true;
            foreach (var sub in s.subQuests)
            {
                sub.isDone = true;
            }
        }
        UpdateQuestDisplay();
        _ = SaveQuestsAsync();
    }
    public async Task SaveQuestsAsync()
    {
        try
        {
            var main = new SerializableList<QuestData>(new List<QuestData>());
            foreach (var q in quests)
            {
                QuestData qd = new QuestData { questName = q.text, isDone = q.isDone };
                foreach (var sub in q.subQuests)
                {
                    qd.subQuests.Add(new QuestData { questName = sub.text, isDone = sub.isDone });
                }
                main.list.Add(qd);
            }

            var side = new SerializableList<QuestData>(new List<QuestData>());
            foreach (var s in sideQuests)
            {
                QuestData sd = new QuestData { questName = s.text, isDone = s.isDone };
                foreach (var sub in s.subQuests)
                {
                    sd.subQuests.Add(new QuestData { questName = sub.text, isDone = sub.isDone });
                }
                side.list.Add(sd);
            }

            string jsonMain = JsonUtility.ToJson(main);
            string jsonSide = JsonUtility.ToJson(side);

            // save local caches
            string mainFile = Path.Combine(Application.persistentDataPath, "mainQuests.json");
            string sideFile = Path.Combine(Application.persistentDataPath, "sideQuests.json");
            File.WriteAllText(mainFile, jsonMain);
            File.WriteAllText(sideFile, jsonSide);

            // save to cloud
            await CloudManager.Instance.SaveToCloudAsJSONAsync("mainQuests", jsonMain);
            await CloudManager.Instance.SaveToCloudAsJSONAsync("sideQuests", jsonSide);

            Debug.Log($"Quests saved (main={main.list.Count} side={side.list.Count})");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to save quests: {e}");
        }
    }

    // Explicit cloud-only save wrapper (does not write local cache)
    public async Task SaveQuestsToCloudAsync()
    {
        try
        {
            var cloudMain = new SerializableList<QuestData>(new List<QuestData>());
            foreach (var q in quests)
            {
                QuestData qd = new QuestData { questName = q.text, isDone = q.isDone };
                foreach (var sub in q.subQuests)
                {
                    qd.subQuests.Add(new QuestData { questName = sub.text, isDone = sub.isDone });
                }
                cloudMain.list.Add(qd);
            }

            var cloudSide = new SerializableList<QuestData>(new List<QuestData>());
            foreach (var s in sideQuests)
            {
                QuestData sd = new QuestData { questName = s.text, isDone = s.isDone };
                foreach (var sub in s.subQuests)
                {
                    sd.subQuests.Add(new QuestData { questName = sub.text, isDone = sub.isDone });
                }
                cloudSide.list.Add(sd);
            }

            string jsonMain = JsonUtility.ToJson(cloudMain);
            string jsonSide = JsonUtility.ToJson(cloudSide);

            await CloudManager.Instance.SaveToCloudAsJSONAsync("mainQuests", jsonMain);
            await CloudManager.Instance.SaveToCloudAsJSONAsync("sideQuests", jsonSide);

            Debug.Log($"Quests cloud-saved (main={cloudMain.list.Count} side={cloudSide.list.Count})");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to cloud-save quests: {e}");
        }
    }

    // Explicit cloud-only load wrapper (applies loaded quests if found)
    public async Task<bool> LoadQuestsFromCloudAsync()
    {
        try
        {
            string jsonMain = await CloudManager.Instance.LoadFromJSONCloud("mainQuests");
            string jsonSide = await CloudManager.Instance.LoadFromJSONCloud("sideQuests");

            if (string.IsNullOrEmpty(jsonMain) && string.IsNullOrEmpty(jsonSide))
            {
                Debug.LogWarning("No quest data found in cloud.");
                return false;
            }

            SerializableList<QuestData> cloudMain = null;
            SerializableList<QuestData> cloudSide = null;

            if (!string.IsNullOrEmpty(jsonMain)) cloudMain = JsonUtility.FromJson<SerializableList<QuestData>>(jsonMain);
            if (!string.IsNullOrEmpty(jsonSide)) cloudSide = JsonUtility.FromJson<SerializableList<QuestData>>(jsonSide);

            SerializableList<QuestData> merged = new SerializableList<QuestData>();
            if (cloudMain != null) merged.list.AddRange(cloudMain.list);
            if (cloudSide != null) merged.list.AddRange(cloudSide.list);

            ApplyLoadedQuests(merged);
            Debug.Log("Quests loaded from cloud and applied.");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load quests from cloud: {e}");
            return false;
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
                 ActivateQuestObject(currentQuestIndex,true);
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
                //  currentSideQuestIndex++;
                //  ActivateQuestObject(currentSideQuestIndex);
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

    public void ActivateQuestObject(int index, bool isMain)
    {
        if(isMain)
        {
            for (int i = 0; i < quests.Count; i++)
            {
                if (quests[i].questUIObject != null)
                    quests[i].questUIObject.SetActive(i == index);
            }
        }
        else
        {
            for (int i = 0; i < sideQuests.Count; i++)
            {
                if (sideQuests[i].questUIObject != null)
                    sideQuests[i].questUIObject.SetActive(i == index);
            }
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

    public void AddNewQuest(Quest questData, bool isMainQuest, bool isSubquest = false, int subQuestIndex = 0, bool isSideQuest = false)
    {
        // Check if there are any duplicate with UI
        List<QuestUI> shownQuest = new List<QuestUI>();
        if(isMainQuest)
        {
            shownQuest = questUIManager.panelMainQuestList.GetComponentsInChildren<QuestUI>().ToList();
            SetCurrentQuestIndex(quests.FindIndex(x=>x==questData));
        }
        else
        {
            shownQuest = questUIManager.panelSubQuestList.GetComponentsInChildren<QuestUI>().ToList();
            SetCurrentSideQuestIndex(sideQuests.FindIndex(x=>x==questData));
        }
        if(!shownQuest.Exists(x=>x.quest.text == questData.text))
        {
            GameObject newItem = Instantiate(questUIManager.questItemPrefab);

            newItem.GetComponent<QuestUI>().SetQuest(questData);

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
        }
        Transform targetTransform = null;
        if(isSubquest)
        {
            targetTransform = questData.subQuests[subQuestIndex].targetTransform;
        }
        else
        {
            targetTransform = questData.targetTransform;
        }
        if (targetTransform!= null)
        {
            questPathManager.SetQuestTarget(targetTransform);
            Debug.Log($"🎯 Quest target diatur ke: {targetTransform}");
        }
        else
        {
            Debug.LogWarning($"⚠️ Quest '{questData.text}' tidak memiliki targetTransform!");
        }
    }
    public void UpdateCurrentQuestInfo(Quest questData, bool isMainQuest, string AddOnSubQuest)
    {
        List<QuestUI> shownQuest = new List<QuestUI>();
        if(isMainQuest)
        {
            shownQuest = questUIManager.panelMainQuestList.GetComponentsInChildren<QuestUI>().ToList();
        }
        else
        {
            shownQuest = questUIManager.panelSubQuestList.GetComponentsInChildren<QuestUI>().ToList();
        }
        var currentQuest = shownQuest.Find(x=>x.quest.text == questData.text);
        if(currentQuest != null)
        {
            Transform subQuestParent = currentQuest.transform.Find("Content");

            if (subQuestParent == null)
            {
                Debug.LogError("Parent untuk subquest tidak ditemukan di prefab! Pastikan ada child bernama Content");
            }

            for (int i = 0; i < questData.subQuests.Count; i++)
            {
                Quest sub = questData.subQuests[i];

                if (sub.questUIObject != null) continue;

                GameObject subItem = subQuestParent.GetChild(i).gameObject;
                TMP_Text subText = subItem.GetComponentInChildren<TMP_Text>();
                subText.text = sub.text;

                // simpan referensi subquest → supaya bisa di-update nanti
                sub.questUIObject = subItem;
                sub.questText = subText;
                sub.questOutline = subItem.GetComponent<Outline>();
            }

            GameObject addOnSubItem = subQuestParent.GetChild(questData.subQuests.Count).gameObject;
            if(addOnSubItem == null)
            {
                addOnSubItem = Instantiate(questUIManager.subQuestItemPrefab, subQuestParent);
            }
            TMP_Text addOnSubText = addOnSubItem.GetComponentInChildren<TMP_Text>();
            addOnSubText.text = AddOnSubQuest;
            addOnSubItem.SetActive(string.IsNullOrEmpty(AddOnSubQuest));
        }
    }
    #region CheatCode
    public void HandleMainQuestCheatCode()
    {
        foreach (var mainQuest in main.list)
        {
            foreach (var subQuest in mainQuest.subQuests)
            {
                subQuest.isDone = true;
            }
            mainQuest.isDone = true;
        }
        ApplyLoadedQuests(main);
        CheckAutoCompleteQuests();
        UpdateQuestDisplay();
        _ = SaveQuestsAsync();
    }
    public void HandleSideQuestCheatCode()
    {
        foreach (var sideQuest in side.list)
        {
            foreach (var subQuest in sideQuest.subQuests)
            {
                subQuest.isDone = true;
            }
            sideQuest.isDone = true;
        }
        ApplyLoadedQuests(side);
        CheckAutoCompleteQuests();
        UpdateQuestDisplay();
        _ = SaveQuestsAsync();
    }
    public void HandleQuestCheatCode(string questName, bool isMain)
    {
        if(isMain)
        {
            var selectedQuest = main.list.Find(x=>x.questName == questName);
            if(selectedQuest == null) return;
            foreach (var subQuest in selectedQuest.subQuests)
            {
                subQuest.isDone = true;
            }
            selectedQuest.isDone = true;
        }
        else
        {
            var selectedQuest = side.list.Find(x=>x.questName == questName);
            if(selectedQuest == null) return;
            foreach (var subQuest in selectedQuest.subQuests)
            {
                subQuest.isDone = true;
            }
            selectedQuest.isDone = true;
        }
        
        CheckAutoCompleteQuests();
        UpdateQuestDisplay();
        _ = SaveQuestsAsync();
    }
    #endregion
}

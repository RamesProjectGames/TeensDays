using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;


public class GameManager : MonoBehaviour
{
    public static GameManager Instance;  
    public PlayerData playerData;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Debug.Log("Masuk dont destroy");
            Instance = this;
            DontDestroyOnLoad(gameObject);            
        }
        else
        {
            Destroy(gameObject);
        }
        
        // Initialize playerData to avoid null reference errors
        if (playerData == null)
        {
            playerData = new PlayerData();
        }
        
        // Initialize default values for the player data
        InitializeDefaultPlayerData();
    }
     
    public int expLevel;
    public int expOverflow;
    public bool kuisDone;
 
    public List<bool> checkLevelCompleted;
    public int totalLevel = 5;
    public delegate void MyDelegate();
    public MyDelegate onLoadDataComplete;
    private void InitializeDefaultPlayerData()
    {
        playerData.expLevel = 0;
        playerData.expOverflow = 0;
        playerData.kuisDone = false;
        playerData.currMoney = 0;
        playerData.currDiamond = 0;
        playerData.checkLevelCompleted = new SerializableList<bool>();
        playerData.ownedItems = new SerializableList<string>();
        playerData.mailboxData = new SerializableList<MailMessage>();
        playerData.mainQuests = new SerializableList<QuestData>();
        playerData.sideQuests = new SerializableList<QuestData>();
        playerData.classExp = 0;
        playerData.questIndex = 0;
        playerData.sideQuestIndex = 0;
        playerData.unlockedLevel = 1;
        
        // Daily reward system defaults
        playerData.currentDay = 1;
        playerData.lastClaimDate = System.DateTime.Now.ToString();
        playerData.specialRewardClaimed = false;
        
        // Audio Settings defaults
        playerData.bgmVolume = 1f;
        playerData.sfxVolume = 1f;
        
        // UI Settings defaults
        playerData.invertCamera = false;
    }

    private void SyncPlayerDataToGame()
    {
        expLevel = playerData.expLevel;
        expOverflow = playerData.expOverflow;
        kuisDone = playerData.kuisDone;
        checkLevelCompleted = playerData.checkLevelCompleted.list;
        if(QuestSystem.instance != null)
        {
            QuestSystem.instance.SetCurrentQuestIndex(playerData.questIndex);
            QuestSystem.instance.SetCurrentSideQuestIndex(playerData.sideQuestIndex);     
            QuestSystem.instance.LoadQuests();       
        }
        if(InventoryManager.Instance != null)
        {
            InventoryManager.Instance.LoadInventory();            
        }
        // Note: currMoney and currDiamond are accessed directly from playerData when needed
        
        // Sync other settings
        // Audio settings will be handled by GlobalAudioManager on Start
        // UI settings will be handled by respective managers
    }

    private void SyncGameToPlayerData()
    {
        playerData.expLevel = expLevel;
        playerData.expOverflow = expOverflow;
        playerData.kuisDone = kuisDone;
        playerData.checkLevelCompleted = new SerializableList<bool>(checkLevelCompleted);
        playerData.questIndex = QuestSystem.instance == null ? 0 : QuestSystem.instance.GetCurrentQuestIndex();
        playerData.sideQuestIndex = QuestSystem.instance == null ? 0 :QuestSystem.instance.GetCurrentSideQuestIndex();
        playerData.ownedItems = new SerializableList<string>(InventoryManager.Instance == null ? new List<string>() : InventoryManager.Instance.ownedItems);
        playerData.mailboxData = new SerializableList<MailMessage>(MailBoxManager.Instance == null ? new List<MailMessage>() : MailBoxManager.Instance.mailboxData.messages);
        playerData.mainQuests = new SerializableList<QuestData>(QuestSystem.instance == null ? new List<QuestData>() : QuestSystem.instance.questDatas);
        playerData.sideQuests = new SerializableList<QuestData>(QuestSystem.instance == null ? new List<QuestData>() : QuestSystem.instance.sideQuestDatas);
    }

    private void Start()
    {
        Application.targetFrameRate = 60;
        LoadPlayerDataFromCloud();
    }
    [ContextMenu("Load Player Data from Cloud")]
    private async void LoadPlayerDataFromCloud()
    {
        try
        {
            string jsonData = await CloudManager.Instance.LoadFromJSONCloud("playerData");
            if (!string.IsNullOrEmpty(jsonData))
            {
                PlayerData loadedData = JsonUtility.FromJson<PlayerData>(jsonData);
                if (loadedData != null)
                {
                    playerData = loadedData;
                    SyncPlayerDataToGame();
                    Debug.Log("Player data loaded from cloud successfully.");
                }
                else
                {
                    Debug.LogWarning("Failed to deserialize player data from cloud.");
                    InitializeDefaultPlayerData();
                    // Save the initialized data to cloud so it persists for next time
                    SavePlayerDataToCloud();
                }
            }
            else
            {
                Debug.LogWarning("No player data found in cloud, using default values.");
                InitializeDefaultPlayerData();
                // Save the initialized data to cloud so it persists for next time
                SavePlayerDataToCloud();
            }
            onLoadDataComplete?.Invoke();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load player data from cloud: {e}");
            InitializeDefaultPlayerData();
            // Save the initialized data to cloud so it persists for next time
            SavePlayerDataToCloud();
            onLoadDataComplete?.Invoke();
        }
    }
    [ContextMenu("Save Player Data to Cloud")]
    public void SavePlayerDataToCloud()
    {
        SyncGameToPlayerData();
        CloudManager.Instance.SaveToCloudAsJSON("playerData", JsonUtility.ToJson(playerData));
        Debug.Log("Player data saved to cloud.");
    }

    public void SaveLevelStatus()
    {
        // Update playerData from current gameplay variables
        for (int i = 0; i < checkLevelCompleted.Count; i++)
        {
            if (i < playerData.checkLevelCompleted.Count())
            {
                playerData.checkLevelCompleted.list[i] = checkLevelCompleted[i];
            }
        }
        
        // Save to cloud
        SavePlayerDataToCloud();
    }

    private void Update()
    {
 
    }
    void OnApplicationPause(bool pause)
    {
        if(pause)
        {
            SavePlayerDataToCloud();
        }
    }
    void OnApplicationQuit()
    {
        SavePlayerDataToCloud();
    }
}

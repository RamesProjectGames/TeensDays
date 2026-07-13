using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cinemachine;


public class GameManager : MonoBehaviour
{
    [System.Serializable]
    private class OwnedItemsLookup
    {
        public SerializableList<string> ownedItems;
    }
    public static GameManager Instance;  
    public PlayerData playerData;
    
    private void Awake()
    {
        transform.SetParent(null); // Ensure GameManager is at root level in hierarchy
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
    public List<int> checkLevelRetries;
    public int totalLevel = 5;
    public delegate void MyDelegate();
    public MyDelegate onLoadDataComplete;

    [Header("References")]
    public PlayerInteraction playerInteraction;
    public CinemachineFreeLook freeLookCamera;
    [ContextMenu("Reset Player Data")]
    private void InitializeDefaultPlayerData()
    {
        Debug.Log("[InitializeDefaultPlayerData] Initializing default player data.");
        playerData.displayName = string.IsNullOrEmpty(playerData.displayName) ? $"Player #{UnityEngine.Random.Range(1000, 9999)}" : playerData.displayName;
        playerData.replaceNameCooldown = 0;
        playerData.playerIconIndex = 0;
        playerData.expLevel = 0;
        playerData.kuisDone = false;
        playerData.unlockedSMP= false;
        playerData.unlockedSMA = false;
        playerData.currMoney = 0;
        playerData.currDiamond = 0;
        playerData.currentSkinId = "default_skin"; // Set a default skin ID
        playerData.checkLevelCompleted = new SerializableList<bool>();
        playerData.levelRetries = new SerializableList<int>();      
        playerData.ownedItems = new List<string>();
        playerData.firstPurchase = new List<string>();
        playerData.mailboxData = new SerializableList<MailMessage>();
        // playerData.mainQuests = new SerializableList<QuestData>();
        // playerData.sideQuests = new SerializableList<QuestData>();
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

        // Saved transform defaults (use current scene objects when available)
        if (playerInteraction == null)
        {
            playerInteraction = FindObjectOfType<PlayerInteraction>();
        }

        if (playerInteraction != null && playerInteraction.playerTransform != null)
        {
            playerData.playerPosition = playerInteraction.playerTransform.localPosition;
            playerData.playerRotation = playerInteraction.playerTransform.localRotation;
        }
        else
        {
            playerData.playerPosition = Vector3.zero;
            playerData.playerRotation = Quaternion.identity;
        }

        if (freeLookCamera != null)
        {
            // Prefer storing the camera's world rotation as Euler for easier inspection
            playerData.cameraPosition = Camera.main != null ? Camera.main.transform.localPosition : Vector3.zero;
            playerData.cameraEuler = Camera.main != null ? Camera.main.transform.eulerAngles : Vector3.zero;
            // store exact FreeLook axis values
            // playerData.cameraXValue = freeLook.m_XAxis.Value;
            playerData.cameraYValue = freeLookCamera.m_YAxis.Value;
        }
        else
        {
            playerData.cameraPosition = Camera.main != null ? Camera.main.transform.localPosition : Vector3.zero;
            playerData.cameraEuler = Camera.main != null ? Camera.main.transform.eulerAngles : Vector3.zero;
            playerData.cameraYValue = 0f;
        }
        
    }

    private void SyncPlayerDataToGame()
    {        
        expLevel = playerData.expLevel;
        kuisDone = playerData.kuisDone;
        checkLevelCompleted = playerData.checkLevelCompleted.list;
        checkLevelRetries = playerData.levelRetries.list;
        for(int i = 0; i < checkLevelCompleted.Count; i++)
        {
            if(!checkLevelCompleted[i])
            {
                playerData.unlockedLevel = i + 1;
                break;
            }
        }
        // if(QuestSystem.instance != null)
        // {
        //     QuestSystem.instance.LoadQuests();       
        //     QuestSystem.instance.SetCurrentSideQuestIndex(playerData.sideQuestIndex);
        // }
        if(InventoryManager.Instance != null)
        {
            InventoryManager.Instance.LoadInventory();            
        }
        var listItemOnShop = FindObjectOfType<ListItem>();
        if(listItemOnShop != null)
        {
            listItemOnShop.StartCoroutine(listItemOnShop.LoadShopItems());
        }
        if(CostumeManager.Instance != null)
        {
            CostumeManager.Instance.LoadCurrentSkin();
        }
        if(FOGManager.Instance != null)
        {
            FOGManager.Instance.LoadBuilding();
        }

        CheckAchievements();

        if (playerInteraction != null && playerInteraction.playerTransform != null)
        {
            playerInteraction.playerTransform.position = playerData.playerPosition;
            playerInteraction.playerTransform.rotation = playerData.playerRotation;
        }
        
        if (freeLookCamera != null)
        {
            // Restore exact FreeLook axes if saved
            Vector3 savedForward =
                Quaternion.Euler(playerData.cameraEuler) * Vector3.forward;

            Vector3 flatForward =
                Vector3.ProjectOnPlane(savedForward, Vector3.up);

            float angle =
                Vector3.SignedAngle(
                    playerInteraction.playerTransform.forward,
                    flatForward,
                    Vector3.up);

            freeLookCamera.m_XAxis.Value = angle;
            freeLookCamera.m_YAxis.Value = playerData.cameraYValue;
        }
        if (Camera.main != null)
        {
            Camera.main.transform.localPosition = playerData.cameraPosition;
            Camera.main.transform.rotation = Quaternion.Euler(playerData.cameraEuler);
        }

        // Note: currMoney and currDiamond are accessed directly from playerData when needed
        
        // Sync other settings
        // Audio settings will be handled by GlobalAudioManager on Start
        // UI settings will be handled by respective managers
    }

    private void EnsurePlayerDataFields()
    {
        if (playerData == null) playerData = new PlayerData();

        if (playerData.checkLevelCompleted == null)
            playerData.checkLevelCompleted = new SerializableList<bool>();
        else if (playerData.checkLevelCompleted.list == null)
            playerData.checkLevelCompleted.list = new List<bool>();

        if (playerData.levelRetries == null)
            playerData.levelRetries = new SerializableList<int>();
        else if (playerData.levelRetries.list == null)
            playerData.levelRetries.list = new List<int>();

        if (playerData.ownedItems == null)
            playerData.ownedItems = new List<string>();

        if (playerData.firstPurchase == null)
            playerData.firstPurchase = new List<string>();

        if (playerData.mailboxData == null)
            playerData.mailboxData = new SerializableList<MailMessage>();
        else if (playerData.mailboxData.list == null)
            playerData.mailboxData.list = new List<MailMessage>();
    }

    private void SyncGameToPlayerData()
    {        
        playerData.expLevel = expLevel;
        playerData.kuisDone = kuisDone;
        playerData.checkLevelCompleted = new SerializableList<bool>(checkLevelCompleted);
        for(int i = 0; i < checkLevelCompleted.Count; i++)
        {
            if(!checkLevelCompleted[i])
            {
                playerData.unlockedLevel = i + 1;
                break;
            }
        }
        playerData.levelRetries = new SerializableList<int>(checkLevelRetries);
        playerData.questIndex = QuestSystem.instance == null ? 0 : QuestSystem.instance.GetCurrentQuestIndex();
        if (playerInteraction != null && playerInteraction.playerTransform != null)
        {
            playerData.playerPosition = playerInteraction.playerTransform.localPosition;
            playerData.playerRotation = playerInteraction.playerTransform.rotation;
        }

        if (freeLookCamera != null)
        {
            // Save camera as Euler to make it easy to inspect and also store exact FreeLook axes
            playerData.cameraPosition = Camera.main != null ? Camera.main.transform.localPosition : Vector3.zero;
            playerData.cameraEuler = Camera.main != null ? Camera.main.transform.eulerAngles : Vector3.zero;
            // playerData.cameraXValue = freeLookCamera.m_XAxis.Value;
            playerData.cameraYValue = freeLookCamera.m_YAxis.Value;
        }
        if (Camera.main != null)
        {
            playerData.cameraPosition = Camera.main.transform.localPosition;
            playerData.cameraEuler = Camera.main.transform.eulerAngles;
        }

        playerData.sideQuestIndex = QuestSystem.instance == null ? 0 :QuestSystem.instance.GetCurrentSideQuestIndex();
        // Preserve existing ownedItems if InventoryManager is not available during save (avoid clearing on quit)
        playerData.ownedItems = InventoryManager.Instance == null ? (playerData.ownedItems ?? new List<string>()) : InventoryManager.Instance.ownedItems.Distinct().ToList();
        playerData.mailboxData = new SerializableList<MailMessage>(MailBoxManager.Instance == null ? new List<MailMessage>() : MailBoxManager.Instance.mailboxData.messages);
        // Quest data is managed by QuestSystem; don't serialize quest lists into PlayerData here.
        // playerData.mainQuests = new SerializableList<QuestData>(new List<QuestData>());
        // playerData.sideQuests = new SerializableList<QuestData>(new List<QuestData>());
    }
    public async void CheckAchievements()
    {
        if(AchieveManager.Instance == null) return;
        if(playerData.unlockedLevel > 6)
        {
            AchieveManager.Instance.UnlockAchievement("sd_achievement");
        }
        if(playerData.unlockedLevel > 9)
        {
            AchieveManager.Instance.UnlockAchievement("smp_achievement");            
        }
        if(playerData.unlockedLevel >= 12)
        {
            if(playerData.checkLevelCompleted.list[playerData.unlockedLevel-1])
            {
                AchieveManager.Instance.UnlockAchievement("sma_achievement");
            }
        }
        if(LeaderboardSystem.Instance ==  null) return;
        var playerScores = await LeaderboardSystem.Instance.GetPlayerLeaderboardScore();
        if(!(playerScores.Count > 12 || playerScores.Count ==0))
        {
            foreach (var score in playerScores)
            {
                if(score < 90)
                {
                    break;
                }
                AchieveManager.Instance.UnlockAchievement("90_score");
            }
        }
    }
    
    private void Start()
    {
        freeLookCamera = FindObjectOfType<CinemachineFreeLook>();
        LoadPlayerDataFromCloud();
    }
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode arg1)
    {
        if(scene.name == "MainMenu")
        {
            // Logic khusus MainMenu
            Debug.Log("Logic khusus MainMenu dijalankan");
            // Misalnya, reset beberapa variabel atau tampilkan UI tertentu
        }
        else if(scene.name == "PortalSD")
        {
            // Logic khusus PortalSD
            Debug.Log("Logic khusus PortalSD dijalankan");
            // Misalnya, set posisi player atau inisialisasi level tertentu
        }
        else if(scene.name == "Dwiky" || scene.name == "Steven")
        {
            // Logic khusus Dwiky
            Debug.Log("Logic khusus Dwiky dijalankan");      
            freeLookCamera = FindObjectOfType<CinemachineFreeLook>();      
            // Misalnya, set posisi player atau inisialisasi level tertentu
            ApplySavedTransforms();
            
        }
    }

    private void ApplySavedTransforms()
    {
        if (playerInteraction == null)
        {
            playerInteraction = FindObjectOfType<PlayerInteraction>();
        }

        if (playerInteraction != null && playerInteraction.playerTransform != null)
        {
            playerInteraction.playerTransform.position = playerData.playerPosition;
            playerInteraction.playerTransform.rotation = playerData.playerRotation;
        }

        if (freeLookCamera != null)
        {
            freeLookCamera.PreviousStateIsValid = false;
            Vector3 savedForward =
                Quaternion.Euler(playerData.cameraEuler) * Vector3.forward;

            Vector3 flatForward =
                Vector3.ProjectOnPlane(savedForward, Vector3.up);

            float angle =
                Vector3.SignedAngle(
                    playerInteraction.playerTransform.forward,
                    flatForward,
                    Vector3.up);

            freeLookCamera.m_XAxis.Value = angle;
            freeLookCamera.m_YAxis.Value = playerData.cameraYValue;
        }
        if (Camera.main != null)
        {
            Camera.main.transform.rotation = Quaternion.Euler(playerData.cameraEuler);
        }
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
                    EnsurePlayerDataFields();

                    // Backwards compatibility: previously ownedItems was SerializableList<string>
                    // which serialized to { "ownedItems": { "list": [ ... ] } }.
                    // If we loaded and `ownedItems` is null, try parsing that shape.
                    if (playerData.ownedItems == null)
                    {
                        try
                        {
                            // temporary lookup struct matching the old JSON shape
                            var lookup = JsonUtility.FromJson<OwnedItemsLookup>(jsonData);
                            if (lookup != null && lookup.ownedItems != null && lookup.ownedItems.list != null)
                            {
                                playerData.ownedItems = new List<string>(lookup.ownedItems.list);
                            }
                        }
                        catch (System.Exception)
                        {
                            // ignore and continue
                        }
                    }

                    // Ensure default skin is present and remove duplicates
                    if (playerData.ownedItems == null)
                        playerData.ownedItems = new List<string>();
                    if (!playerData.ownedItems.Contains("default_skin"))
                    {
                        playerData.ownedItems.Insert(0, "default_skin");
                    }
                    playerData.ownedItems = playerData.ownedItems.Distinct().ToList();

                    // Sync to InventoryManager if available (deduplicated)
                    if (InventoryManager.Instance != null)
                    {
                        InventoryManager.Instance.ownedItems = playerData.ownedItems.Distinct().ToList();
                    }

                    Debug.Log($"[LoadPlayerDataFromCloud] ownedItems count={playerData.ownedItems?.Count ?? 0} items={string.Join(",", playerData.ownedItems ?? new List<string>())}");
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
            SyncPlayerDataToGame();
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
        Debug.Log($"[SavePlayerDataToCloud] ownedItems count={playerData.ownedItems?.Count ?? 0} items={string.Join(",", playerData.ownedItems ?? new List<string>())}");
        CloudManager.Instance.SaveToCloudAsJSON("playerData", JsonUtility.ToJson(playerData));
        Debug.Log("Player data saved to cloud.");
    }

    public async Task SavePlayerDataToCloudAsync()
    {
        SyncGameToPlayerData();
        await CloudManager.Instance.SaveToCloudAsJSONAsync("playerData", JsonUtility.ToJson(playerData));
        Debug.Log("Player data saved to cloud (async).");
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
        else
        {
            LoadPlayerDataFromCloud();
        }
    }
    void OnApplicationQuit()
    {
        SavePlayerDataToCloud();
    }

    public void UnlockedAllSegmentClass()
    {
        if (!playerData.unlockedSMP)
        {
            for (int i = 0; i < 6; i++)
            {
                playerData.checkLevelCompleted.list[i] = true;
                playerData.levelRetries.list[i] = 3;
            }
            playerData.unlockedLevel = 7;
        }
        else if (!playerData.unlockedSMA)
        {
            for (int i = 6; i < 12; i++)
            {
                playerData.checkLevelCompleted.list[i] = true;
                playerData.levelRetries.list[i] = 3;
            }
            playerData.unlockedLevel = 12;
        }
        if (QuestSystem.instance != null)
        {
            QuestSystem.instance.HandleMainQuestCheatCode();
            QuestSystem.instance.HandleSideQuestCheatCode();
        }
        SyncPlayerDataToGame();
    }
}

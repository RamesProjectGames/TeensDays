
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerData 
{
    // User Identification
    public string userId;
    public string displayName;
    public long replaceNameCooldown = 0;
    public int playerIconIndex = 0;
    
    // Game Progress
    public int expLevel;
    public bool kuisDone;
    public bool unlockedSMP;
    public bool unlockedSMA;
    public int currMoney;
    public int currDiamond;
    public string currentSkinId = "default_skin";
    public SerializableList<bool> checkLevelCompleted = new SerializableList<bool>();
    public SerializableList<int> levelRetries = new SerializableList<int>();
    public List<string> ownedItems = new List<string>();
    public List<string> firstPurchase = new List<string>();
    public SerializableList<MailMessage> mailboxData = new SerializableList<MailMessage>();
    // public SerializableList<QuestData> mainQuests = new SerializableList<QuestData>();
    // public SerializableList<QuestData> sideQuests = new SerializableList<QuestData>();
    public int classExp;
    public int questIndex;
    public int sideQuestIndex;
    public int unlockedLevel;
    
    // Daily reward system
    public int currentDay;
    public string lastClaimDate;
    public bool specialRewardClaimed;
    
    // Audio Settings
    public float bgmVolume;
    public float sfxVolume;
    
    // UI Settings
    public bool invertCamera;

    // Persisted transform data
    public Vector3 playerPosition;
    public Quaternion playerRotation;
    public Vector3 cameraPosition;
    // store camera rotation as Euler angles to make it easy to inspect in Inspector
    public Vector3 cameraEuler;
    // store Cinemachine FreeLook axis values for exact restore
    public float cameraXValue;
    public float cameraYValue;
}

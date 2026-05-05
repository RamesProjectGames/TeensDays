
using UnityEngine;

[System.Serializable]
public class PlayerData 
{
    // User Identification
    public string userId;
    public string displayName;
    
    // Game Progress
    public int expLevel;
    public int expOverflow;
    public bool kuisDone;
    public int currMoney;
    public int currDiamond;
    public SerializableList<bool> checkLevelCompleted = new SerializableList<bool>();
    public SerializableList<string> ownedItems = new SerializableList<string>();
    public SerializableList<MailMessage> mailboxData = new SerializableList<MailMessage>();
    public SerializableList<QuestData> mainQuests = new SerializableList<QuestData>();
    public SerializableList<QuestData> sideQuests = new SerializableList<QuestData>();
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
}


using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "PlayerData", order = 0)]
[System.Serializable]
public class PlayerData : ScriptableObject 
{
   // User Identification
   public string userId;
   
   // Game Progress
   public int expLevel;
   public int expOverflow;
   public bool kuisDone;
   public int currMoney;
   public int currDiamond;
   public bool[] checkLevelCompleted;
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

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

[System.Serializable]
public class Quest
{
    public TMP_Text questText;
    public string text;
    public string description;
    public int expForQuest;
    public Transform targetTransform;
    public GameObject questObjectAnnoun;
    public GameObject npcObject;
    public bool isDone;
    public bool isUnlocked = true;
    public string questAssetAddressKey;
    public AssetReference assetReference;
    [HideInInspector] public AsyncOperationHandle<GameObject> assetHandle;
    [HideInInspector] public bool hasAssetDownloaded;
    [HideInInspector] public GameObject spawnedAsset;

    [Header("Quest Teleport")]
    public bool autoTeleportOnStart = true;
    public string teleportDestinationName;
    public QuestTeleportTarget teleportTarget;

    public List<Quest> subQuests = new List<Quest>(); // Tambahkan ini

    [Header("Side Quest Unlocks")]
    public List<string> sideQuestsToUnlock = new List<string>();

    public AssignmentManager assignmentManager; // Tambahkan ini

    public GameObject questUIObject;   // referensi prefab UI
    public Outline questOutline;       // referensi Outline UI
    public List<QuestReward> questRewards = new List<QuestReward>();

    public bool IsFullyCompleted()
    {
        // Quest utama selesai jika dirinya dan semua subquest selesai
        return isDone && subQuests.All(sq => sq.isDone);
    }
}
[System.Serializable]
public class QuestData
{
    public string questName;
    public bool isDone;
    public bool isUnlocked = true;
    public List<QuestData> subQuests = new List<QuestData>();
    public bool IsFullyCompleted()
    {
        // Quest utama selesai jika dirinya dan semua subquest selesai
        return isDone && subQuests.All(sq => sq.isDone);
    }
}
[System.Serializable]
public class QuestReward
{
    public string rewardId;
    public int rewardAmount;
    public Sprite rewardIcon;
    public QuestRewardType type;
}
public enum QuestRewardType
{
    Money,
    Diamonds
}

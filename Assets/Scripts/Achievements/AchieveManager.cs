using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class AchieveManager : MonoBehaviour
{
    private static AchieveManager instance = null;
    public static AchieveManager Instance => instance;

    [SerializeField] private AchievementScriptableObject[] defaultAchievements;
    public Image achieveAnnoun;
    public AudioSource achieveAnnounAudio;
    
    public List<AchievementData> playerAchievements = new List<AchievementData>();
    private Dictionary<string, AchievementScriptableObject> achievementLookup = new Dictionary<string, AchievementScriptableObject>();

    public event Action<AchievementData> OnAchievementUnlocked;
    public event Action<AchievementData> OnAchievementClaimed;
    public event Action<List<AchievementData>> OnAchievementsLoaded;

    void Awake()
    {
        transform.SetParent(null);
        if (instance == null)
        {
            Debug.Log("Masuk dont destroy");
            instance = this;
            DontDestroyOnLoad(gameObject);            
        }
        else
        {
            Destroy(gameObject);
        }
        InitializeAchievementLookup();
    }

    void Start()
    {
        StartCoroutine(LoadAchievementsFromCloud());
    }

    private void InitializeAchievementLookup()
    {
        foreach (var achievement in defaultAchievements)
        {
            achievementLookup[achievement.AchievementId] = achievement;
        }
    }

    private IEnumerator LoadAchievementsFromCloud()
    {
        yield return new WaitUntil(() => AuthenticationManager.Singleton?.auth?.CurrentUser != null);
        
        var task = CloudManager.Instance.LoadFromJSONCloud("achievements");
        yield return new WaitUntil(() => task.IsCompleted);

        if (!task.IsFaulted && task.Result != null)
        {
            try
            {
                SerializableList<AchievementData> achievementList = JsonUtility.FromJson<SerializableList<AchievementData>>(task.Result);
                playerAchievements = achievementList.list ?? new List<AchievementData>();
                Debug.Log($"Loaded {playerAchievements.Count} achievements from cloud");
            }
            catch (Exception e)
            {
                Debug.LogError($"Error loading achievements: {e.Message}");
                InitializeDefaultAchievements();
            }
        }
        else
        {
            InitializeDefaultAchievements();
        }

        OnAchievementsLoaded?.Invoke(playerAchievements);
    }

    private void InitializeDefaultAchievements()
    {
        playerAchievements.Clear();
        foreach (var achievement in defaultAchievements)
        {
            playerAchievements.Add(achievement.ToAchievementData(false));
        }
        SaveAchievementsToCloud();
    }

    public void UnlockAchievement(string achievementId)
    {
        var achievement = playerAchievements.Find(a => a.achievementId == achievementId);
        
        if (achievement == null)
        {
            Debug.LogWarning($"Achievement {achievementId} not found");
            return;
        }

        if (!achievement.isClaimed)
        {
            achievement.obtainedTimestamp = System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            SaveAchievementsToCloud();
            OnAchievementUnlocked?.Invoke(achievement);
            ShowAchievePanel(achievementId);
            Debug.Log($"Achievement unlocked: {achievement.title}");
        }
    }

    public void ClaimReward(int _index)
    {
        if (_index < 0 || _index >= playerAchievements.Count)
        {
            Debug.LogWarning($"Invalid achievement index: {_index}");
            return;
        }

        var achievementId = playerAchievements[_index].achievementId;
        ClaimAchievement(achievementId);
    }

    public void ClaimAchievement(string achievementId)
    {
        var achievement = playerAchievements.Find(a => a.achievementId == achievementId);
        
        if (achievement == null)
        {
            Debug.LogWarning($"Achievement {achievementId} not found");
            return;
        }

        if (achievement.isClaimed)
        {
            Debug.LogWarning($"Achievement {achievementId} already claimed");
            return;
        }

        achievement.isClaimed = true;

        if (achievementLookup.TryGetValue(achievementId, out var achievementScriptable))
        {
            GameManager.Instance.playerData.currMoney += achievementScriptable.RewardAmount;
            GameManager.Instance.SavePlayerDataToCloud();
            Debug.Log($"Achievement claimed: {achievementScriptable.Title}, reward: {achievementScriptable.RewardAmount}");
        }

        SaveAchievementsToCloud();
        OnAchievementClaimed?.Invoke(achievement);
    }

    public void SaveAchievementsToCloud()
    {
        SerializableList<AchievementData> achievementList = new SerializableList<AchievementData> { list = playerAchievements };
        string jsonData = JsonUtility.ToJson(achievementList);
        CloudManager.Instance.SaveToCloudAsJSON("achievements", jsonData);
        Debug.Log("Achievements saved to cloud");
    }

    public void ShowAchievePanel(string achievementId = "")
    {
        if (achieveAnnoun == null || achieveAnnounAudio == null)
        {
            Debug.LogWarning("Achievement announcement UI components not assigned");
            return;
        }

        achieveAnnounAudio.Play();
        LeanTween.moveY(achieveAnnoun.rectTransform, 280, .7f).setOnComplete(() =>
        {
            LeanTween.delayedCall(5f, () =>
            {
                LeanTween.moveY(achieveAnnoun.rectTransform, 430, .7f);
            });
        });
    }

    public AchievementData GetAchievement(string achievementId)
    {
        return playerAchievements.Find(a => a.achievementId == achievementId);
    }

    public List<AchievementData> GetAllAchievements() => new List<AchievementData>(playerAchievements);

    public List<AchievementData> GetUnclaimedAchievements()
    {
        return playerAchievements.FindAll(a => !a.isClaimed && a.obtainedTimestamp > 0);
    }

    public List<AchievementData> GetClaimedAchievements()
    {
        return playerAchievements.FindAll(a => a.isClaimed);
    }

    public AchievementScriptableObject GetAchievementScriptable(string achievementId)
    {
        achievementLookup.TryGetValue(achievementId, out var achievement);
        return achievement;
    }
}

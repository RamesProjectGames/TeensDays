using Firebase.Database;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
public enum LeaderboardGroup
{
    Class_1_6,
    Class_7_9,
    Class_10_12
}
public partial class LeaderboardSystem : MonoBehaviour
{
    public static LeaderboardSystem Instance { get; private set; }
    private DatabaseReference leaderboardRef;
    public event Action<List<LeaderboardData>> OnLeaderboardUpdated;

    private void Awake()
    {
        transform.SetParent(null);
        if (Instance == null)
        {
            Instance = this;
        }
    }
    private void Start()
    {
        leaderboardRef = FirebaseDatabase.DefaultInstance.GetReference("leaderboard");

        StartListening();
    }

    private void OnDestroy()
    {
        StopListening();
    }
    private void OnEnable()
    {
        OnLeaderboardUpdated += UpdateUI;
    }

    private void OnDisable()
    {
        OnLeaderboardUpdated -= UpdateUI;
    }

    void UpdateUI(List<LeaderboardData> players)
    {
        foreach (var p in players)
        {
            Debug.Log($"{p.userId} - {p.bestTime}");
        }
    }
    // 🔥 START LISTENING (REAL-TIME)
    public void StartListening()
    {
        leaderboardRef
            .OrderByChild("bestTime")
            .LimitToFirst(50)
            .ValueChanged += OnLeaderboardValueChanged;
    }

    public void StopListening()
    {
        if (leaderboardRef != null)
            leaderboardRef.ValueChanged -= OnLeaderboardValueChanged;
    }

    private void OnLeaderboardValueChanged(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }

        List<LeaderboardData> list = new List<LeaderboardData>();

        foreach (var child in args.Snapshot.Children)
        {
            var data = JsonUtility.FromJson<LeaderboardData>(child.GetRawJsonValue());
            list.Add(data);
        }

        OnLeaderboardUpdated?.Invoke(list);
    }
    public async Task<long> StartRun()
    {
        var user = AuthenticationManager.Singleton.auth.CurrentUser;
        string userId = user.UserId;

        var startData = new Dictionary<string, object>
        {
            { "startTime", ServerValue.Timestamp }
        };

        await leaderboardRef.Child("runs").Child(userId).SetValueAsync(startData);

        return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(); // local reference
    }
    public async void SubmitScoreValidated(int highestClass)
    {
        var user = AuthenticationManager.Singleton.auth.CurrentUser;
        string userId = user.UserId;

        var snapshot = await leaderboardRef
            .Child("runs")
            .Child(userId)
            .GetValueAsync();

        if (!snapshot.Exists)
        {
            Debug.LogWarning("Run not started");
            return;
        }

        long startTime = (long)snapshot.Child("startTime").Value;

        long currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        long duration = currentTime - startTime;

        // 🔒 Anti-cheat checks
        if (!IsValidTime(duration))
        {
            Debug.LogWarning("Cheat detected");
            return;
        }

        await SubmitScoreSafe(highestClass, duration);
    }
    private bool IsValidTime(long timeMs)
    {
        // 🚫 Too fast (impossible)
        if (timeMs < 5000) return false; // < 5 sec

        // 🚫 Too long (AFK / tampered)
        if (timeMs > 3600000) return false; // > 1 hour

        return true;
    }
    public async Task SubmitScore(int highestClass, long bestTimeMs)
    {
        var user = AuthenticationManager.Singleton.auth.CurrentUser;

        if (user == null)
        {
            Debug.LogWarning("User not logged in");
            return;
        }

        string userId = user.UserId;

        LeaderboardData data = new LeaderboardData
        {
            userId = userId,
            highestClass = highestClass,
            bestTime = bestTimeMs,
            sortKey = GenerateSortKey(highestClass, bestTimeMs)
        };

        string json = JsonUtility.ToJson(data);

        await leaderboardRef
            .Child("leaderboard")
            .Child(userId)
            .SetRawJsonValueAsync(json);

        Debug.Log("Score submitted to leaderboard");
    }

    // 🔹 Safe submit (only if better)
    public async Task SubmitScoreSafe(int highestClass, long bestTimeMs)
    {
        var user = AuthenticationManager.Singleton.auth.CurrentUser;
        if (user == null) return;

        string userId = user.UserId;

        var snapshot = await leaderboardRef
            .Child("leaderboard")
            .Child(userId)
            .GetValueAsync();

        if (snapshot.Exists)
        {
            var existing = JsonUtility.FromJson<LeaderboardData>(snapshot.GetRawJsonValue());

            if (bestTimeMs >= existing.bestTime)
            {
                Debug.Log("Not a better score");
                return;
            }
        }

        await SubmitScore(highestClass, bestTimeMs);
    }

    // 🔹 Get Top Players (Best Time)
    public async Task<List<LeaderboardData>> GetTopPlayers(int limit = 10)
    {
        List<LeaderboardData> result = new List<LeaderboardData>();

        var snapshot = await leaderboardRef
            .Child("leaderboard")
            .OrderByChild("sortKey")
            .LimitToFirst(limit)
            .GetValueAsync();

        if (!snapshot.Exists) return result;

        foreach (var child in snapshot.Children)
        {
            var data = JsonUtility.FromJson<LeaderboardData>(child.GetRawJsonValue());
            result.Add(data);
        }

        return result;
    }

    // 🔹 Get Top by Class
    public async Task<List<LeaderboardData>> GetTopByClass(int limit = 10)
    {
        List<LeaderboardData> result = new List<LeaderboardData>();

        var snapshot = await leaderboardRef
            .Child("leaderboard")
            .OrderByChild("highestClass")
            .LimitToLast(limit)
            .GetValueAsync();

        if (!snapshot.Exists) return result;

        foreach (var child in snapshot.Children)
        {
            var data = JsonUtility.FromJson<LeaderboardData>(child.GetRawJsonValue());
            result.Add(data);
        }

        result.Reverse(); // highest first
        return result;
    }

    // 🔹 Get Player Rank
    public async Task<int> GetPlayerRank(string userId)
    {
        var snapshot = await leaderboardRef
            .Child("leaderboard")
            .OrderByChild("bestTime")
            .GetValueAsync();

        int rank = 1;

        foreach (var child in snapshot.Children)
        {
            if (child.Key == userId)
                return rank;

            rank++;
        }

        return -1;
    }
    private LeaderboardGroup GetGroup(int highestClass)
    {
        if (highestClass <= 6) return LeaderboardGroup.Class_1_6;
        if (highestClass <= 9) return LeaderboardGroup.Class_7_9;
        return LeaderboardGroup.Class_10_12;
    }
    public class GroupedLeaderboard
    {
        public List<LeaderboardData> class1_6 = new();
        public List<LeaderboardData> class7_9 = new();
        public List<LeaderboardData> class10_12 = new();
    }
    public async Task<GroupedLeaderboard> GetGroupedLeaderboard(int limitPerGroup = 10)
    {
        var allPlayers = await GetTopPlayers(100); // fetch enough

        GroupedLeaderboard grouped = new GroupedLeaderboard();

        foreach (var p in allPlayers)
        {
            var group = GetGroup(p.highestClass);

            switch (group)
            {
                case LeaderboardGroup.Class_1_6:
                    if (grouped.class1_6.Count < limitPerGroup)
                        grouped.class1_6.Add(p);
                    break;

                case LeaderboardGroup.Class_7_9:
                    if (grouped.class7_9.Count < limitPerGroup)
                        grouped.class7_9.Add(p);
                    break;

                case LeaderboardGroup.Class_10_12:
                    if (grouped.class10_12.Count < limitPerGroup)
                        grouped.class10_12.Add(p);
                    break;
            }
        }

        return grouped;
    }
    private string GenerateSortKey(int highestClass, long bestTime)
    {
        // invert class so higher = smaller value (for ascending sort)
        int invertedClass = 999999 - highestClass;

        // pad both values to fixed length
        return $"{invertedClass:D6}_{bestTime:D12}";
    }
    public async Task DeleteFromLeaderboardFull()
    {
        var user = AuthenticationManager.Singleton.auth.CurrentUser;

        if (user == null)
        {
            Debug.LogWarning("User not logged in");
            return;
        }

        string userId = user.UserId;

        // 🔍 Get existing data to know group
        var snapshot = await leaderboardRef
            .Child("leaderboard")
            .Child(userId)
            .GetValueAsync();

        if (!snapshot.Exists)
        {
            Debug.Log("User not in leaderboard");
            return;
        }

        var data = JsonUtility.FromJson<LeaderboardData>(snapshot.GetRawJsonValue());

        string groupKey = GetGroupKey(data.highestClass);

        // 🔥 Multi-path delete (atomic)
        var updates = new Dictionary<string, object>()
        {
            [$"leaderboard/{userId}"] = null,
            [$"groupedLeaderboard/{groupKey}/{userId}"] = null
        };

        await leaderboardRef.UpdateChildrenAsync(updates);

        Debug.Log("Deleted from ALL leaderboard nodes");
    }
    private string GetGroupKey(int highestClass)
    {
        if (highestClass <= 6) return "class1_6";
        if (highestClass <= 9) return "class7_9";
        return "class10_12";
    }
}

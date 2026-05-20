using Firebase.Database;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using UnityEngine;

public class LeaderboardSystem : MonoBehaviour
{
    public static LeaderboardSystem Instance { get; private set; }

    private DatabaseReference rootRef;

    public event Action<List<LeaderboardData>> OnLeaderboardUpdated;

    private Query currentQuery;

    public event Action<int> OnPlayerRankUpdated;

    private Query playerRankQuery;

    private EventHandler<ValueChangedEventArgs> playerRankHandler;

    private void Awake()
    {
        transform.SetParent(null);

        if (Instance == null)
        {
            Instance = this;
        }

        rootRef = FirebaseDatabase.DefaultInstance.RootReference;
    }

    void Start()
    {
        
    }
    // =========================
    // REALTIME LISTENER
    // =========================

    public void ListenToClassLeaderboard(int playerClass)
    {
        StopListening();

        string seasonKey = SeasonUtility.GetCurrentSeasonKey();

        currentQuery = rootRef
            .Child("leaderboards")
            .Child(seasonKey)
            .Child($"class_{playerClass}")
            .OrderByChild("sortKey")
            .LimitToFirst(10);

        currentQuery.ValueChanged += OnLeaderboardChanged;
    }

    public void StopListening()
    {
        if (currentQuery != null)
        {
            currentQuery.ValueChanged -= OnLeaderboardChanged;
            currentQuery = null;
        }
    }

    public void ListenToPlayerCurrentRank(int playerClass)
    {
        var user = AuthenticationManager.Singleton.auth.CurrentUser;

        if (user == null)
        {
            Debug.LogWarning("User not logged in");
            return;
        }

        StopPlayerRankListening();

        string uid = user.UserId;

        string seasonKey = SeasonUtility.GetCurrentSeasonKey();

        playerRankQuery = rootRef
            .Child("leaderboards")
            .Child(seasonKey)
            .Child($"class_{playerClass}")
            .OrderByChild("sortKey");

        playerRankHandler = (sender, args) =>
        {
            if (args.DatabaseError != null)
            {
                Debug.LogError(args.DatabaseError.Message);
                return;
            }

            int rank = 1;

            bool found = false;

            foreach (var child in args.Snapshot.Children)
            {
                if (child.Key == uid)
                {
                    found = true;

                    OnPlayerRankUpdated?.Invoke(rank);

                    break;
                }

                rank++;
            }

            if (!found)
            {
                OnPlayerRankUpdated?.Invoke(-1);
            }
        };

        playerRankQuery.ValueChanged += playerRankHandler;
    }

    public void StopPlayerRankListening()
    {
        if (playerRankQuery != null && playerRankHandler != null)
        {
            playerRankQuery.ValueChanged -= playerRankHandler;
        }

        playerRankQuery = null;

        playerRankHandler = null;
    }

    private void OnDestroy()
    {
        StopListening();
    }

    private void OnLeaderboardChanged(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }

        List<LeaderboardData> players = new();

        foreach (var child in args.Snapshot.Children)
        {
            LeaderboardData data = JsonUtility.FromJson<LeaderboardData>(
                child.GetRawJsonValue()
            );

            players.Add(data);
        }

        OnLeaderboardUpdated?.Invoke(players);
    }// =========================
    // RUN START
    // =========================
    
    public async Task StartRun()
    {
        var user = AuthenticationManager.Singleton.auth.CurrentUser;

        if (user == null)
        {
            Debug.LogWarning("User not logged in");
            return;
        }

        string userId = user.UserId;

        Dictionary<string, object> startData = new()
        {
            { "startTime", ServerValue.Timestamp }
        };

        await rootRef
            .Child("runs")
            .Child(userId)
            .SetValueAsync(startData);
    }

    // =========================
    // SUBMIT VALIDATED SCORE
    // =========================

    async Task SubmitScoreTaskValidated(
        int playerClass,
        int playerIconIndex
    )
    {
        var user = AuthenticationManager.Singleton.auth.CurrentUser;

        if (user == null)
        {
            Debug.LogWarning("User not logged in");
            return;
        }

        string userId = user.UserId;

        var snapshot = await rootRef
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

        if (!IsValidTime(duration))
        {
            Debug.LogWarning("Cheat detected");
            return;
        }

        await SubmitScore(
            playerClass,
            duration,
            playerIconIndex
        );
    }
    public async void SubmitScoreValidated(int playerClass, int playerIconIndex)
    {
        await SubmitScoreTaskValidated(
            playerClass,
            playerIconIndex
        );
    }

    private bool IsValidTime(long timeMs)
    {
        if (timeMs < 5000)
            return false;

        if (timeMs > 3600000)
            return false;

        return true;
    }// =========================
    // SUBMIT SCORE
    // =========================

    public async Task SubmitScore(
        int playerClass,
        long bestTimeMs,
        int playerIconIndex
    )
    {
        var user = AuthenticationManager.Singleton.auth.CurrentUser;

        if (user == null)
        {
            Debug.LogWarning("User not logged in");
            return;
        }

        string userId = user.UserId;

        string seasonKey = SeasonUtility.GetCurrentSeasonKey();

        LeaderboardData data = new()
        {
            userId = userId,
            playerClass = playerClass,
            bestTime = bestTimeMs,
            playerIconIndex = playerIconIndex,
            rewardAmount = GetReward(bestTimeMs),
            sortKey = GenerateSortKey(bestTimeMs)
        };

        string json = JsonUtility.ToJson(data);

        await rootRef
            .Child("leaderboards")
            .Child(seasonKey)
            .Child($"class_{playerClass}")
            .Child(userId)
            .SetRawJsonValueAsync(json);

        Debug.Log("Score submitted");
    }

    // =========================
    // GET TOP PLAYERS
    // =========================

    public async Task<List<LeaderboardData>> GetTopPlayers(
        int playerClass,
        int limit = 10
    )
    {
        List<LeaderboardData> result = new();

        string seasonKey = SeasonUtility.GetCurrentSeasonKey();

        var snapshot = await rootRef
            .Child("leaderboards")
            .Child(seasonKey)
            .Child($"class_{playerClass}")
            .OrderByChild("sortKey")
            .LimitToFirst(limit)
            .GetValueAsync();

        if (!snapshot.Exists)
            return result;

        foreach (var child in snapshot.Children)
        {
            LeaderboardData data = JsonUtility.FromJson<LeaderboardData>(
                child.GetRawJsonValue()
            );

            result.Add(data);
        }

        return result;
    }

    public async Task<LeaderboardData> GetPlayerData(int playerClass)
    {
        var user = AuthenticationManager.Singleton.auth.CurrentUser;

        if (user == null)
        {
            Debug.LogWarning("User not logged in");
            return null;
        }

        string seasonKey = SeasonUtility.GetCurrentSeasonKey();
        string uid = user.UserId;

        var snapshot = await rootRef
            .Child("leaderboards")
            .Child(seasonKey)
            .Child($"class_{playerClass}")
            .Child(uid)
            .GetValueAsync();

        if (!snapshot.Exists)
            return null;

        LeaderboardData data = JsonUtility.FromJson<LeaderboardData>(
            snapshot.GetRawJsonValue()
        );

        return data;
    }

    // =========================
    // DELETE PLAYER ENTRY
    // =========================

    public async Task DeleteEntry(int playerClass)
    {
        var user = AuthenticationManager.Singleton.auth.CurrentUser;

        if (user == null)
            return;

        string seasonKey = SeasonUtility.GetCurrentSeasonKey();

        await rootRef
            .Child("leaderboards")
            .Child(seasonKey)
            .Child($"class_{playerClass}")
            .Child(user.UserId)
            .RemoveValueAsync();
    }
    public async Task DeletePlayerFromAllLeaderboards(string uid)
    {
        string seasonKey = SeasonUtility.GetCurrentSeasonKey();

        Dictionary<string, object> updates = new();

        for (int i = 1; i <= 12; i++)
        {
            updates[
                $"leaderboards/{seasonKey}/class_{i}/{uid}"
            ] = null;
        }

        await rootRef.UpdateChildrenAsync(updates);

        Debug.Log("Deleted player from all leaderboards.");
    }

    // =========================
    // UTILITIES
    // =========================

    private string GenerateSortKey(long bestTime)
    {
        return bestTime.ToString("D12");
    }

    private int GetReward(long timeMs)
    {
        if (timeMs <= 60000)
            return 1000;

        if (timeMs <= 120000)
            return 500;

        return 100;
    }
}
public static class SeasonUtility
{
    public static string GetCurrentSeasonKey()
    {
        var calendar = CultureInfo.InvariantCulture.Calendar;

        int week = calendar.GetWeekOfYear(
            DateTime.UtcNow,
            CalendarWeekRule.FirstFourDayWeek,
            DayOfWeek.Monday
        );

        return $"season_{DateTime.UtcNow.Year}_week_{week}";
    }
}

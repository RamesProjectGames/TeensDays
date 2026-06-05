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
    public event Action OnPlayerDataFetched;

    public List<int> rankRewards = new() { 1000, 750, 500, 300, 200, 100, 75, 50, 25, 10 };
    public int defaultRankReward = 0;
    public event Action<List<LeaderboardData>> OnLeaderboardUpdated;
    public event Action<string> OnSeasonChanged;

    private string currentSeasonKey;
    private int currentClassListening = 1;
    private bool isListeningToLeaderboard;
    private bool isListeningToPlayerRank;

    private Query currentQuery;

    public event Action<int> OnPlayerRankUpdated;

    private Query playerRankQuery;

    private EventHandler<ValueChangedEventArgs> playerRankHandler;

    private void Awake()
    {

        if (Instance == null)
        {
            Instance = this;
        }

        rootRef = FirebaseDatabase.DefaultInstance.RootReference;
        currentSeasonKey = SeasonUtility.GetCurrentSeasonKey();
    }

    void Start()
    {
    }

    private void Update()
    {
        CheckSeasonChange();
    }

    private void CheckSeasonChange()
    {
        string seasonKey = SeasonUtility.GetCurrentSeasonKey();

        if (seasonKey == currentSeasonKey)
            return;

        string previousSeasonKey = currentSeasonKey;
        currentSeasonKey = seasonKey;
        HandleSeasonChanged(previousSeasonKey);
    }

    private async void HandleSeasonChanged(string previousSeasonKey)
    {
        Debug.Log($"Season changed to {currentSeasonKey}, resetting leaderboard listeners.");

        await SendSeasonEndMailForPlayer(previousSeasonKey);

        StopListening();
        StopPlayerRankListening();

        OnLeaderboardUpdated?.Invoke(new List<LeaderboardData>());
        OnSeasonChanged?.Invoke(currentSeasonKey);

        if (isListeningToLeaderboard)
            ListenToClassLeaderboard(currentClassListening);

        if (isListeningToPlayerRank)
            ListenToPlayerCurrentRank(currentClassListening);
    }
    // =========================
    // REALTIME LISTENER
    // =========================

    public void ListenToClassLeaderboard(int playerClass)
    {
        StopListening();

        currentClassListening = playerClass;
        isListeningToLeaderboard = true;

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

        isListeningToLeaderboard = false;
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

        currentClassListening = playerClass;
        isListeningToPlayerRank = true;

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
        isListeningToPlayerRank = false;
    }

    private void OnDestroy()
    {
        StopListening();
        StopPlayerRankListening();
    }

    private void OnLeaderboardChanged(object sender, ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError(args.DatabaseError.Message);
            return;
        }

        List<LeaderboardData> players = new();

        int rank = 1;
        foreach (var child in args.Snapshot.Children)
        {
            LeaderboardData data = JsonUtility.FromJson<LeaderboardData>(
                child.GetRawJsonValue()
            );

            data.rewardAmount = GetRewardForRank(rank);
            players.Add(data);
            rank++;
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
        string displayName,
        int playerClass,
        int playerScore,
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
            displayName,
            playerClass,
            duration,
            playerScore,
            playerIconIndex
        );
    }
    public async void SubmitScoreValidated(string displayName, int playerClass, int playerScore, int playerIconIndex)
    {
        await SubmitScoreTaskValidated(
            displayName,
            playerClass,
            playerScore,
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
        string displayName,
        int playerClass,
        long bestTimeMs,
        int playerScore,
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
            displayName = displayName,
            bestTime = bestTimeMs,
            score = playerScore,
            playerIconIndex = playerIconIndex,
            rewardAmount = 0,
            sortKey = GenerateSortKey(playerScore, bestTimeMs)
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

            data.rewardAmount = GetRewardForRank(result.Count + 1);
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

    public async Task<LeaderboardData> GetPlayerDataForSeason(int playerClass, string seasonKey)
    {
        var user = AuthenticationManager.Singleton.auth.CurrentUser;

        if (user == null)
        {
            Debug.LogWarning("User not logged in");
            return null;
        }

        string uid = user.UserId;

        var snapshot = await rootRef
            .Child("leaderboards")
            .Child(seasonKey)
            .Child($"class_{playerClass}")
            .Child(uid)
            .GetValueAsync();

        if (!snapshot.Exists)
            return null;

        return JsonUtility.FromJson<LeaderboardData>(snapshot.GetRawJsonValue());
    }

    public async Task<int> GetPlayerRankForSeason(int playerClass, string seasonKey)
    {
        var user = AuthenticationManager.Singleton.auth.CurrentUser;

        if (user == null)
        {
            Debug.LogWarning("User not logged in");
            return -1;
        }

        string uid = user.UserId;

        var snapshot = await rootRef
            .Child("leaderboards")
            .Child(seasonKey)
            .Child($"class_{playerClass}")
            .OrderByChild("sortKey")
            .GetValueAsync();

        if (!snapshot.Exists)
            return -1;

        int rank = 1;
        foreach (var child in snapshot.Children)
        {
            if (child.Key == uid)
                return rank;
            rank++;
        }

        return -1;
    }

    private async Task SendSeasonEndMailForPlayer(string previousSeasonKey)
    {
        if (string.IsNullOrEmpty(previousSeasonKey))
            return;

        var user = AuthenticationManager.Singleton.auth.CurrentUser;
        if (user == null)
            return;

        if (GameManager.Instance == null || GameManager.Instance.playerData == null)
            return;

        string uid = user.UserId;

        int highestUnlockedClass = GameManager.Instance.playerData.unlockedLevel;
        if (highestUnlockedClass < 1)
            return;
        
        highestUnlockedClass = Mathf.Min(highestUnlockedClass, 12);

        int rank = await GetPlayerRankForSeason(highestUnlockedClass, previousSeasonKey);
        if (rank <= 0)
            return;

        SeasonRewardSummary highestSeasonReward = new SeasonRewardSummary
        {
            playerClass = highestUnlockedClass,
            rank = rank,
            rewardAmount = GetRewardForRank(rank)
        };

        string seasonName = SeasonUtility.GetSeasonDisplayName(previousSeasonKey);
        string title = $"Season {seasonName} Reward";
        string body = ComposeSeasonEndMailBody(seasonName, highestSeasonReward);

        var rewardList = new List<object>
        {
            new Dictionary<string, object>
            {
                { "type", "Money" },
                { "amount", highestSeasonReward.rewardAmount },
                { "classId", highestSeasonReward.playerClass },
                { "rank", highestSeasonReward.rank }
            }
        };

        string messageId = Guid.NewGuid().ToString("N");
        long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        long expiresAt = DateTimeOffset.UtcNow.AddDays(30).ToUnixTimeMilliseconds();

        var mailData = new Dictionary<string, object>
        {
            { "title", title },
            { "body", body },
            { "startsAt", now },
            { "expiresAt", expiresAt },
            { "isClaimed", false },
            { "isRead", false },
            { "isDeleted", false },
            { "rewards", rewardList }
        };

        await rootRef
            .Child("systemMail")
            .Child(uid)
            .Child(messageId)
            .UpdateChildrenAsync(mailData);

        Debug.Log($"Season end mail sent for {seasonName} to player {uid}");
    }

    private string ComposeSeasonEndMailBody(string seasonName, SeasonRewardSummary reward)
    {
        string intro = $"Season {seasonName} has ended! Your final leaderboard reward is ready in your inbox.";
        string detail = $"Class {reward.playerClass}: Rank {reward.rank} → {reward.rewardAmount} coins";
        string outro = "Claim your reward from the mail panel and keep climbing next season to earn bigger prizes.";

        return $"{intro}\n\n{detail}\n\n{outro}";
    }

    private class SeasonRewardSummary
    {
        public int playerClass;
        public int rank;
        public int rewardAmount;
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

    private string GenerateSortKey(int score, long bestTime)
    {
        long invertedScore = 999999999 - score;

        return $"{invertedScore:D9}_{bestTime:D12}";
    }

    public int GetRewardForRank(int rank)
    {
        if (rank <= 0)
            return defaultRankReward;

        int index = rank - 1;
        if (index < rankRewards.Count)
            return rankRewards[index];

        return defaultRankReward;
    }

    private int GetReward(long timeMs)
    {
        if (timeMs <= 60000)
            return 1000;

        if (timeMs <= 120000)
            return 500;

        return 100;
    }

    // =========================
    // EDITOR DEBUG TOOLS
    // =========================

    [ContextMenu("Fill Leaderboard with Dummy Data")]
    private async void FillLeaderboardDummy()
    {
        Debug.Log("Filling leaderboard with dummy data...");

        string seasonKey = SeasonUtility.GetCurrentSeasonKey();

        try
        {
            for (int classId = 1; classId <= 12; classId++)
            {
                for (int rank = 1; rank <= 10; rank++)
                {
                    long bestTime = 30000 + (rank * 5000);
                    int playerIconIndex = UnityEngine.Random.Range(0, 5);

                    LeaderboardData data = new()
                    {
                        userId = $"dummy_user_{classId}_{rank}",
                        playerClass = classId,
                        bestTime = bestTime,
                        playerIconIndex = playerIconIndex,
                        rewardAmount = GetReward(bestTime),
                        sortKey = GenerateSortKey(0, bestTime)
                    };

                    string json = JsonUtility.ToJson(data);

                    await rootRef
                        .Child("leaderboards")
                        .Child(seasonKey)
                        .Child($"class_{classId}")
                        .Child($"dummy_user_{classId}_{rank}")
                        .SetRawJsonValueAsync(json);
                }
            }

            Debug.Log("Leaderboard filled with dummy data!");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error filling leaderboard: {ex.Message}\nUpdate your Firebase rules to allow dummy_user_ entries.");
        }
    }

    [ContextMenu("Reset Leaderboard")]
    private async void ResetLeaderboard()
    {
        Debug.Log("Resetting dummy leaderboard entries...");

        string seasonKey = SeasonUtility.GetCurrentSeasonKey();

        try
        {
            for (int classId = 1; classId <= 12; classId++)
            {
                var snapshot = await rootRef
                    .Child("leaderboards")
                    .Child(seasonKey)
                    .Child($"class_{classId}")
                    .GetValueAsync();

                if (!snapshot.Exists)
                    continue;

                foreach (var child in snapshot.Children)
                {
                    // Only delete dummy entries, preserve legitimate player entries
                    if (child.Key.StartsWith("dummy_user_"))
                    {
                        await rootRef
                            .Child("leaderboards")
                            .Child(seasonKey)
                            .Child($"class_{classId}")
                            .Child(child.Key)
                            .RemoveValueAsync();
                    }
                }
            }

            Debug.Log("Dummy leaderboard entries reset!");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error resetting leaderboard: {ex.Message}\nCheck your Firebase Realtime Database rules to allow delete access.");
        }
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

    public static string GetSeasonDisplayName(string seasonKey)
    {
        if (string.IsNullOrEmpty(seasonKey))
            return "unknown";

        var parts = seasonKey.Split('_');
        if (parts.Length == 4 && int.TryParse(parts[1], out int year))
        {
            return $"Week {parts[3]} of {year}";
        }

        return seasonKey;
    }
}

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LeaderboardUIManager : MonoBehaviour
{
    public static LeaderboardUIManager Instance { get; private set; }

    [Header("UI")]
    public List<Sprite> rankBackgrounds = new List<Sprite>();
    [Header("References")]
    public Transform contentParent;

    public LeaderboardEntryUI entryPrefab;

    [Header("Class Buttons")]
    public Button[] classButtons;

    [Header("Season")]
    public TMP_Text seasonText;

    private readonly List<LeaderboardEntryUI> spawnedEntries = new();


    private int currentClass = 1;

    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        if(seasonText != null) seasonText.text = SeasonUtility.GetCurrentSeasonKey();

        for (int i = 0; i < classButtons.Length; i++)
        {
            int classIndex = i + 1;

            classButtons[i].onClick.AddListener(() =>
            {
                ChangeClass(classIndex);
            });
        }

        LeaderboardSystem.Instance.OnLeaderboardUpdated += RefreshUI;

        ChangeClass(1);
    }

    private void OnDestroy()
    {
        if (LeaderboardSystem.Instance != null)
        {
            LeaderboardSystem.Instance.OnLeaderboardUpdated -= RefreshUI;
        }
    }

    public async void ChangeClass(int playerClass)
    {
        currentClass = playerClass;

        LeaderboardSystem.Instance.ListenToClassLeaderboard(playerClass);

        List<LeaderboardData> players =
            await LeaderboardSystem.Instance.GetTopPlayers(playerClass, 10);

        RefreshUI(players);
    }

    private void RefreshUI(List<LeaderboardData> players)
    {
        ClearUI();

        for (int i = 0; i < players.Count; i++)
        {
            LeaderboardEntryUI entry = Instantiate(
                entryPrefab,
                contentParent
            );

            entry.Setup(i + 1, players[i]);

            spawnedEntries.Add(entry);
        }
    }

    private void ClearUI()
    {
        foreach (var entry in spawnedEntries)
        {
            if (entry != null)
            {
                Destroy(entry.gameObject);
            }
        }

        spawnedEntries.Clear();
    }
}

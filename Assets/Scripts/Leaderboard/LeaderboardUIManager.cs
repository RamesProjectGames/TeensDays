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
    public GameObject emptyLeaderboardMessage;
    [Header("References")]
    public Transform contentParent;

    public LeaderboardEntryUI entryPrefab;

    [Header("Class Buttons")]
    public Button[] classButtons;
    public List<AccordionDropdown> accordionDropdowns = new List<AccordionDropdown>();

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

        classButtons[currentClass - 1].interactable = false;
        for (int i = 0; i < classButtons.Length; i++)
        {
            if (i != currentClass - 1)
            {
                classButtons[i].interactable = true;
                classButtons[i].GetComponent<RectTransform>().localPosition = new Vector3(0, classButtons[i].GetComponent<RectTransform>().localPosition.y, 0);
            }
            else
            {
                classButtons[i].GetComponent<RectTransform>().localPosition = new Vector3(-25, classButtons[i].GetComponent<RectTransform>().localPosition.y, 0);
            }
        }

        LeaderboardSystem.Instance.ListenToClassLeaderboard(playerClass);

        List<LeaderboardData> players =
            await LeaderboardSystem.Instance.GetTopPlayers(playerClass, 10);

        RefreshUI(players);
    }

    private void RefreshUI(List<LeaderboardData> players)
    {
        ClearUI();
        if(players.Count == 0)
        {
            emptyLeaderboardMessage.SetActive(true);
            return;
        }
        else
        {
            emptyLeaderboardMessage.SetActive(false);
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

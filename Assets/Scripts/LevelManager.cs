using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[System.Serializable]
public class LocalTime
{
    public int classId;
    public TMP_Text timeText;
}
public class LevelManager : MonoBehaviour
{
    public int minClass;
    public Button[] levelButtons;
    public LocalTime[] levelRetries;
    public GameObject[] bgSpritesLocked;
    public GameObject[] bgNomorKelas;
    public GameObject[] bgKelasClear;
    public LocalTime[] bestTimes;

    private async void Start()
    {
        await UpdateBestTimes();
    }

    private void Awake()
    {
        UpdateLevelButtons();
        GetLevelRetries();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void OpenLevel(int levelId)
    {
        string prevSceneName = SceneManager.GetActiveScene().name;
        int prevSceneIndex = SceneManager.GetActiveScene().buildIndex;

        SceneManager.sceneLoaded += OnSceneLoaded;

        SceneManager.LoadScene(levelId);
    }
    public void OpenLevel(string levelName)
    {
        string prevSceneName = SceneManager.GetActiveScene().name;
        int prevSceneIndex = SceneManager.GetActiveScene().buildIndex;

        SceneManager.sceneLoaded += OnSceneLoaded;

        SceneManager.LoadScene(levelName);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {

        if (SceneEntryManager.LastEntryID != "PortalSD")
            return;

        Debug.Log("Logic khusus PortalSD dijalankan");
        //SceneManager.sceneLoaded -= OnSceneLoaded;
        //if (scene.name == "Rama") 
        //{
        //    if (PlayerMovement.Instance != null)
        //    {
        //        PlayerMovement.Instance.objectPlayerSpawn.transform.position = new Vector3(77, 3, 6);
        //        Debug.Log("Player dipindahkan ke posisi awal!");
        //    }
        //    else
        //    {
        //        Debug.LogWarning("PlayerMovement.Instance tidak ditemukan di scene baru!");
        //    }
        //}
    }

    public void ResetLevel()
    {
        // Clear player data and save to cloud (effectively resetting progress)
        if (GameManager.Instance != null && GameManager.Instance.playerData != null)
        {
            // Reset to default values
            GameManager.Instance.playerData.unlockedLevel = 1;
            // Reset other progress data as needed
            // GameManager.Instance.SavePlayerDataToCloud();
        }
    }

    public void UpdateLevelButtons()
    {
        int unlockedLevel = 1;
        if (GameManager.Instance != null && GameManager.Instance.playerData != null)
        {
            unlockedLevel = GameManager.Instance.playerData.unlockedLevel - minClass;
        }

        for (int i = 0; i < levelButtons.Length; i++)
        {
            levelButtons[i].interactable = false;
            bgSpritesLocked[i].SetActive(true);
        }
        for (int i = 0; i < unlockedLevel && i < levelButtons.Length; i++)
        {
            levelButtons[i].interactable = true;
            bgSpritesLocked[i].SetActive(false);

            if (i < unlockedLevel - 1)
            {
                bgNomorKelas[i].SetActive(false);
                bgKelasClear[i].SetActive(true);
            }
        }
    }
    public void GetLevelRetries()
    {
        if (GameManager.Instance != null && GameManager.Instance.playerData != null)
        {
            var levelRetriesNumbers = GameManager.Instance.playerData.levelRetries.list;
            for (int i = 0; i < levelRetries.Length; i++)
            {
                int playerClass = levelRetries[i].classId - minClass;
                int retries = levelRetriesNumbers[playerClass];
                levelRetries[i].timeText.text = $"{retries} / 3";
                Debug.Log($"Level {i + 1} retries: {retries}");
            }
        }
    }

    private async Task UpdateBestTimes()
    {
        if (bestTimes == null || bestTimes.Length == 0)
            return;

        if (LeaderboardSystem.Instance == null)
        {
            for (int i = 0; i < bestTimes.Length; i++)
            {
                bestTimes[i].timeText.text = "--:--:---";
            }
            return;
        }

        for (int i = 0; i < bestTimes.Length; i++)
        {
            int playerClass = bestTimes[i].classId;
            LeaderboardData playerData = await LeaderboardSystem.Instance.GetPlayerData(playerClass);
            if (playerData == null)
            {
                bestTimes[i].timeText.text = "--:--:---";
            }
            else
            {
                bestTimes[i].timeText.text = FormatTime(playerData.bestTime);
            }
        }
    }

    private string FormatTime(long ms)
    {
        long minutes = ms / 60000;
        ms %= 60000;

        long seconds = ms / 1000;
        ms %= 1000;

        return $"<b>{minutes:D2}:{seconds:D2}:{ms:D3}</b>";
    }
}


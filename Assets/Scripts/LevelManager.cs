using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    public Button[] levelButtons;
    public Button[] bgButtons;
    public Sprite[] bgSpritesLocked;
    public Sprite[] bgSpritesUnlocked;
    public Sprite[] levelLockedImages;
    public Sprite[] levelUnlockImages;

    private void Start()
    {


    }

    private void Awake()
    {
        int UnlockedLevel = PlayerPrefs.GetInt("UnlockedLevel", 1);

        for (int i = 0; i < levelButtons.Length; i++)
        {
            levelButtons[i].interactable = false;
            levelButtons[i].image.sprite = levelLockedImages[i];
            bgButtons[i].image.sprite = bgSpritesLocked[i];
        }
        for (int i = 0; i < UnlockedLevel; i++)
        {
            levelButtons[i].interactable = true;
            levelButtons[i].image.sprite = levelUnlockImages[i];
            bgButtons[i].image.sprite = bgSpritesUnlocked[i];
        }
    }

    public void OpenLevel(int levelId)
    {
        string prevSceneName = SceneManager.GetActiveScene().name;
        int prevSceneIndex = SceneManager.GetActiveScene().buildIndex;

        SceneManager.sceneLoaded += OnSceneLoaded;

        SceneManager.LoadScene(levelId);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        if (scene.name == "Rama") 
        {
            if (PlayerMovement.Instance != null)
            {
                PlayerMovement.Instance.objectPlayerSpawn.transform.position = new Vector3(77, -2, 6);
                Debug.Log("Player dipindahkan ke posisi awal!");
            }
            else
            {
                Debug.LogWarning("PlayerMovement.Instance tidak ditemukan di scene baru!");
            }
        }
    }

    public void ResetLevel()
    {
        PlayerPrefs.DeleteAll();
    }
}

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    public Button[] levelButtons;
    public GameObject[] bgSpritesLocked;

    private void Start()
    {


    }

    private void Awake()
    {
        UpdateLevelButtons();
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
        PlayerPrefs.DeleteAll();
    }

    public void UpdateLevelButtons()
    {
        int UnlockedLevel = PlayerPrefs.GetInt("UnlockedLevel", 1);

        for (int i = 0; i < levelButtons.Length; i++)
        {
            levelButtons[i].interactable = false;
            bgSpritesLocked[i].SetActive(true);
        }
        for (int i = 0; i < UnlockedLevel && i < levelButtons.Length; i++)
        {
            levelButtons[i].interactable = true;
            bgSpritesLocked[i].SetActive(false);
        }
    }
}

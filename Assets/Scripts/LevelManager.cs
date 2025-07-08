using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    public Button[] levelButtons;

    private void Awake()
    {
        int UnlockedLevel = PlayerPrefs.GetInt("UnlockedLevel", 1);

        for (int i = 0; i < levelButtons.Length; i++)
        {
            levelButtons[i].interactable = false;
        }
        for (int i = 0; i < UnlockedLevel; i++)
        {
            levelButtons[i].interactable = true;
        }
    }

    public void OpenLevel(int levelId)
    {
        SceneManager.LoadScene(levelId);
    }

    public void ResetLevel()
    {
        PlayerPrefs.DeleteAll();
    }
}

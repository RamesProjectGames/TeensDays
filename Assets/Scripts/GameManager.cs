using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;


public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    public int expLevel;
    public int expOverflow;
    public bool kuisDone;

    public int currMoney;
    public int currDiamond;

    public bool[] checkLevelCompleted;
    public int totalLevel = 5;

    private void Awake()
    {
        int savedDiamond = PlayerPrefs.GetInt("Diamond", 0);
        currDiamond = savedDiamond;

        int savedMoney = PlayerPrefs.GetInt("Money", 0);
        currMoney = savedMoney;

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            checkLevelCompleted = new bool[totalLevel];

            for (int i = 0; i < totalLevel; i++)
            {
                checkLevelCompleted[i] = PlayerPrefs.GetInt("LevelCompleted_" + i, 0) == 1;
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
    }

    public void SaveLevelStatus()
    {
        for (int i = 0; i < checkLevelCompleted.Length; i++)
        {
            PlayerPrefs.SetInt("LevelCompleted_" + i, checkLevelCompleted[i] ? 1 : 0);
        }
        PlayerPrefs.Save();
    }

    private void Update()
    {

    }
}

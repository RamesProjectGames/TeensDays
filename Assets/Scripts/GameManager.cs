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

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Agar tetap hidup saat ganti scene
        }
        else
        {
            Destroy(gameObject); // Jika sudah ada, hapus duplikat
        }
    }
}

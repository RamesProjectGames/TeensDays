using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static PlayerManager;

public class PlayerManager : MonoBehaviour
{
    public GameObject[] EnterRoom;

    [Tooltip("Daftar nama scene yang bisa dipilih lewat index")]
    public string[] sceneNames;  // array nama scene

    [Header("Player Stats")]
    //public TMP_Text expPlayer;
    //public TMP_Text limitExpPlayer;
    public TMP_Text classExpPlayer;
    public Slider expSlider;

    [Header("Currency Game")]
    public TMP_Text money_text;
    public TMP_Text diamond_text;


    public bool hasCompleted;

    public List<SekolahUI> sekolahList = new List<SekolahUI>();

    [System.Serializable]
    public class SekolahUI
    {
        public bool isActive;
        public GameObject aktifObj;
        public GameObject pembatasSekolah;
        public GameObject nonAktifObj;
        public GameObject npcObj;
        public int unlockLevel;
    }

    private void Start()
    {
        CekLevelSekolah();
    }

    private void Update()
    {
        money_text.text = GameManager.Instance.playerData.currMoney.ToString();
        diamond_text.text = GameManager.Instance.playerData.currDiamond.ToString();

        if (GameManager.Instance.playerData.kuisDone && !hasCompleted)
        {
            ExpManager();
            hasCompleted = true;
        }

        //expPlayer.text = GameManager.Instance.expLevel.ToString();
        expSlider.value = GameManager.Instance.playerData.expLevel / 100f;
        classExpPlayer.text = GameManager.Instance.playerData.classExp.ToString();
        UpdateSemuaSekolah();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.collider.CompareTag("DoorSystem1"))
        {
            EnterRoom[0].SetActive(true);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.collider.CompareTag("DoorSystem1"))
        {
            EnterRoom[0].SetActive(false);
        }
    }

    public void ExpManager()
    {
        // Update tampilan slider
        expSlider.value = GameManager.Instance.playerData.expLevel;

        if (GameManager.Instance.playerData.expLevel >= 100)
        {
            GameManager.Instance.playerData.expLevel = GameManager.Instance.playerData.expLevel %100;

            GameManager.Instance.playerData.classExp += 1; // Tambah level atau exp class
            hasCompleted = false;
            GameManager.Instance.playerData.kuisDone = false;
        }
    }

    public void LoadSceneByIndex(int index)
    {
        if (index >= 0 && index < sceneNames.Length)
        {
            StartCoroutine(LoadSceneAfterSave(index));
        }
        else
        {
            Debug.LogWarning("Index scene tidak valid: " + index);
        }
    }

    private IEnumerator LoadSceneAfterSave(int index)
    {
        var saveTask = GameManager.Instance.SavePlayerDataToCloudAsync();
        while (!saveTask.IsCompleted)
        {
            yield return null;
        }

        if (saveTask.IsFaulted)
        {
            Debug.LogError($"Failed to save player data before loading scene: {saveTask.Exception}");
            // If save failed, still continue to load scene, but we have a local cache fallback.
        }
        else if (saveTask.IsCompletedSuccessfully)
        {
            Debug.Log("Player data successfully saved before scene load.");
        }

        SceneManager.LoadScene(sceneNames[index]);
    }

    public void UpdateSemuaSekolah()
    {
        foreach (var sekolah in sekolahList)
        {
            sekolah.aktifObj.SetActive(sekolah.isActive);
            sekolah.pembatasSekolah.SetActive(sekolah.isActive);
            sekolah.nonAktifObj.SetActive(!sekolah.isActive);
            sekolah.npcObj.SetActive(!sekolah.isActive);
        }
    }

    public void CekLevelSekolah()
    {
        int unlockedLevel = GameManager.Instance.playerData.unlockedLevel;

        foreach (var sekolah in sekolahList)
        {
            Debug.Log("masuk ke foreach");
            if (unlockedLevel >= sekolah.unlockLevel)
            {
                sekolah.isActive = false;
            }
        }
    }
}

    

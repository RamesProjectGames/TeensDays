using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static Cinemachine.DocumentationSortingAttribute;
using static UnityEngine.InputSystem.LowLevel.InputStateHistory;
using System.Text.RegularExpressions;

[System.Serializable]
public class SoalData
{
    public string soal;
    public string[] jawaban = new string[4];
    public char kunci; // A, B, C, D
}
public class QuizManager : MonoBehaviour
{
    public static QuizManager instance;

    public TextAsset csvFile;

    public AudioSource sucessAudio;
    public AudioSource failAudio;
    public AudioSource correctAudio;
    public AudioSource falseAudio;

    public TMP_Text soalTMP;
    public TMP_Text feedbackTMP;
    public TMP_Text healthTMP;
    public Button[] jawabanButtons; // urutan A, B, C, D
    public Button nextButton;
    public Button BackToLevel;

    public List<SoalData> semuaSoal = new List<SoalData>();
    public int indexSoal;
    private bool answered = false;
    private bool alreadyUnlocked = false;

    public int totalBenar = 0;
    public int totalSalah = 0;
    public int maxHealth;
    public int currHealth;
    long startTime;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {

        LoadCSV();
        ShuffleSoal();
        TampilkanSoal(indexSoal);
        currHealth = maxHealth;
        UpdateHealthUI();
        feedbackTMP.text = "";
        totalBenar = 0;
        totalSalah = 0;
        indexSoal = 0;
        StartTrackTime();
        //BackToLevel.gameObject.SetActive(false);

        for (int i = 0; i < jawabanButtons.Length; i++)
        {
            int pilihanIndex = i;
            jawabanButtons[i].onClick.AddListener(() => CekJawaban(pilihanIndex));
        }
    }
    async void StartTrackTime()
    {
        await LeaderboardSystem.Instance.StartRun();
    }
    void LoadCSV()
    {
        StringReader reader = new StringReader(csvFile.text);
        bool isHeader = true;

        while (reader.Peek() > -1)
        {
            string line = reader.ReadLine();
            if (isHeader) { isHeader = false; continue; }

            string[] values = line.Split(';');
            //Debug.Log(values.Length);
            if (values.Length >= 8)
            {
                //Debug.Log("Masuk sini");
                SoalData soal = new SoalData
                {
                    soal = values[1],
                    jawaban = new string[4] { values[2], values[3], values[4], values[5] },
                    kunci = values[7].ToUpper()[0] // A/B/C/D
                };
                semuaSoal.Add(soal);
            }
        }
    }

    public void NextButton()
    {
        indexSoal++;

        if (indexSoal < semuaSoal.Count)
        {
            TampilkanSoal(indexSoal);
            nextButton.gameObject.SetActive(false);

        }
        else
        {
            soalTMP.text = "Soal selesai!";
            feedbackTMP.text = "";

            foreach (var btn in jawabanButtons)
            {
                btn.gameObject.SetActive(false);
            }

            nextButton.gameObject.SetActive(false);
            TotalScore();
        }
    }

    public void TampilkanSoal(int index)
    {
        SoalData s = semuaSoal[index];
        //Debug.Log("Masuk Tampilkan Soal dan shuffle");
        soalTMP.text = s.soal;
        feedbackTMP.text = "";
        answered = false;

        for (int i = 0; i < 4; i++)
        {
            jawabanButtons[i].GetComponentInChildren<TMP_Text>().text = s.jawaban[i];
            jawabanButtons[i].enabled = true;

            var image = jawabanButtons[i].GetComponent<Image>();

            Color color = new Color(12f / 255f, 49f / 255f, 32f / 255f);

            image.color = color;
        }
    }

    void CekJawaban(int index)
    {
        if (answered) return;

        answered = true;
        char pilihan = (char)('A' + index);
        SoalData current = semuaSoal[indexSoal];
        nextButton.gameObject.SetActive(true);

        if (pilihan == current.kunci)
        {
            //feedbackTMP.text = "<color=green>Jawaban benar!</color>";
            totalBenar++;
            correctAudio.Play();

            jawabanButtons[index].GetComponent<Image>().color = Color.green;
        }
        else
        {
            //feedbackTMP.text = $"<color=red>Salah! Jawaban benar: {current.kunci}</color>";
            totalSalah++;
            falseAudio.Play();

            jawabanButtons[index].GetComponent<Image>().color = Color.red;

            //Kasih Tau Jawaban Benar
            int indexBenar = current.kunci - 'A';
            jawabanButtons[indexBenar].GetComponent<Image>().color = Color.green;

            KurangiHealth();
        }

        // Nonaktifkan tombol agar tidak bisa ditekan lagi
        foreach (var btn in jawabanButtons)
        {
            btn.enabled = false;
        }
    }

    void ShuffleSoal()
    {
        for (int i = 0; i < semuaSoal.Count; i++)
        {
            //Debug.Log("Soal kerandom");
            int rand = Random.Range(i, semuaSoal.Count);
            var temp = semuaSoal[i];
            semuaSoal[i] = semuaSoal[rand];
            semuaSoal[rand] = temp;
        }

        if (semuaSoal.Count > 1)
        {
            semuaSoal = semuaSoal.GetRange(0, 10);
        }

    }

    void TotalScore()
    {
        // int levelIndex = SceneManager.GetActiveScene().buildIndex;
        int currentClass = GetNumberFromString(SceneManager.GetActiveScene().name);

        float skor = ((float)totalBenar / semuaSoal.Count) * 100f;
        soalTMP.text = $"Soal selesai!\nSkor akhir: <color=green>{skor:F0}%</color>\nBenar: {totalBenar}, Salah: {totalSalah}";

        if (skor >= 75 || skor > 75)
        {
            feedbackTMP.text = "<color=green>Lulus!</color>";
            GameManager.Instance.playerData.kuisDone = true;
            BackToLevel.gameObject.SetActive(true);
            sucessAudio.Play();

            if (!GameManager.Instance.playerData.checkLevelCompleted.list[currentClass-1])
            {
                // Pertama kali lulus → hadiah besar
                GameManager.Instance.playerData.currMoney += 15000;
                GameManager.Instance.playerData.checkLevelCompleted.list[currentClass - 1] = true;
                GameManager.Instance.playerData.expLevel += 100;
            }
            else
            {
                // Sudah pernah lulus → hadiah kecil
                GameManager.Instance.playerData.currMoney += 500;
            }

            if (GameManager.Instance.playerData.levelRetries.list[currentClass - 1]< 3)
            {
                GameManager.Instance.playerData.levelRetries.list[currentClass - 1] += 1;
            }

            UnlockNewLevel();
            // Save to cloud instead of PlayerPrefs
            LeaderboardSystem.Instance.SubmitScoreValidated(GameManager.Instance.playerData.displayName, currentClass, (int)skor, GameManager.Instance.playerData.playerIconIndex);
            GameManager.Instance.SavePlayerDataToCloud();
        }
        else
        {
            feedbackTMP.text = "<color=red>Tidak lulus</color>";
            BackToLevel.gameObject.SetActive(true);
            failAudio.Play();
        }
    }

    void UnlockNewLevel()
    {
        // if (alreadyUnlocked) return;
        // alreadyUnlocked = true;

        int currentClass = GetNumberFromString(SceneManager.GetActiveScene().name);
        int unlockedLevel = GameManager.Instance.playerData.unlockedLevel;

        if(currentClass == 6)
        {
            int totalLevelRetries = 0;
            for (int i = 0; i < GameManager.Instance.playerData.levelRetries.list.Count; i++)
            {
                totalLevelRetries += GameManager.Instance.playerData.levelRetries.list[i];
            }
            if(totalLevelRetries >= 18)
            {
                int nextLevel = currentClass + 1;

                if (nextLevel > unlockedLevel)
                {
                    GameManager.Instance.playerData.unlockedLevel = nextLevel;
                    AchievementHelper.UnlockAchievement("sd_achievement");
                    GameManager.Instance.SavePlayerDataToCloud();
                    // Debug.Log("Unlocked new level: " + nextLevel);
                }
            }
        }
        else if(currentClass ==9)
        {
            int totalLevelRetries = 0;
            for (int i = 0; i < GameManager.Instance.playerData.levelRetries.list.Count; i++)
            {
                totalLevelRetries += GameManager.Instance.playerData.levelRetries.list[i];
            }
            if(totalLevelRetries >= 27)
            {
                int nextLevel = currentClass + 1;

                if (nextLevel > unlockedLevel)
                {
                    GameManager.Instance.playerData.unlockedLevel = nextLevel;
                    AchievementHelper.UnlockAchievement("smp_achievement");
                    GameManager.Instance.SavePlayerDataToCloud();
                    // Debug.Log("Unlocked new level: " + nextLevel);
                }
            }
        }
        else if(currentClass < 12)
        {
            int nextLevel = currentClass + 1;

            if (nextLevel > unlockedLevel)
            {
                GameManager.Instance.playerData.unlockedLevel = nextLevel;
                GameManager.Instance.SavePlayerDataToCloud();
                // Debug.Log("Unlocked new level: " + nextLevel);
            }
        }
        else
        {
            AchievementHelper.UnlockAchievement("sma_achievement");
            GameManager.Instance.playerData.unlockedLevel = 12;
            GameManager.Instance.SavePlayerDataToCloud();
        }
    }

    public void BackToLevelScene(int levelId)
    {
        SceneManager.LoadScene(levelId);
    }

    void KurangiHealth()
    {
        currHealth--;

        if (currHealth <= 0)
        {
            GameOver();
        }
        else
        {
            UpdateHealthUI();
        }
    }

    public void UpdateHealthUI()
    {
        healthTMP.text = $"{currHealth}/{maxHealth}";

        //healthSlider.value = (float)currentHealth / maxHealth;
    }

    public void GameOver()
    {
        int sisaSoal = semuaSoal.Count - (indexSoal + 1);
        totalSalah += sisaSoal;
        indexSoal = semuaSoal.Count;
        NextButton();
    }
    public static int GetNumberFromString(string input)
    {
        Match match = Regex.Match(input, @"\d+");
        return match.Success ? int.Parse(match.Value) : 0;
    }
}
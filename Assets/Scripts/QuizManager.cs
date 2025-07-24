using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static Cinemachine.DocumentationSortingAttribute;

[System.Serializable]
public class SoalData
{
    public string soal;
    public string[] jawaban = new string[4];
    public char kunci; // A, B, C, D
}
public class QuizManager : MonoBehaviour
{
    public TextAsset csvFile;

    public TMP_Text soalTMP;
    public TMP_Text feedbackTMP;
    public Button[] jawabanButtons; // urutan A, B, C, D
    public Button nextButton;
    public Button BackToLevel;

    public List<SoalData> semuaSoal = new List<SoalData>();
    private int indexSoal;
    private bool answered = false;
    private bool alreadyUnlocked = false;

    public int totalBenar = 0;
    public int totalSalah = 0;

    void Start()
    {
        LoadCSV();
        ShuffleSoal();
        TampilkanSoal(indexSoal);
        feedbackTMP.text = "";
        totalBenar = 0;
        totalSalah = 0;
        indexSoal = 0;
        //BackToLevel.gameObject.SetActive(false);

        for (int i = 0; i < jawabanButtons.Length; i++)
        {
            int pilihanIndex = i;
            jawabanButtons[i].onClick.AddListener(() => CekJawaban(pilihanIndex));
        }
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
            };
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
            jawabanButtons[i].interactable = true;
        }
    }

    void CekJawaban(int index)
    {
        if (answered) return;

        answered = true;
        char pilihan = (char)('A' + index);
        SoalData current = semuaSoal[indexSoal];

        if (pilihan == current.kunci)
        {
            feedbackTMP.text = "<color=green>Jawaban benar!</color>";
            totalBenar++;
        }
        else
        {
            feedbackTMP.text = $"<color=red>Salah! Jawaban benar: {current.kunci}</color>";
            totalSalah++;
        }

        // Nonaktifkan tombol agar tidak bisa ditekan lagi
        foreach (var btn in jawabanButtons)
        {
            btn.interactable = false;
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

        if (semuaSoal.Count > 5)
        {
            semuaSoal = semuaSoal.GetRange(0, 5); 
        }

    }

    void TotalScore()
    {
        float skor = ((float)totalBenar / semuaSoal.Count) * 100f;
        soalTMP.text = $"Soal selesai!\nSkor akhir: <color=green>{skor:F0}%</color>\nBenar: {totalBenar}, Salah: {totalSalah}";

        if (skor >= 75)
        {
            feedbackTMP.text = "<color=green>Lulus!</color>";
            UnlockNewLevel();
            GameManager.Instance.expLevel += 100;
            GameManager.Instance.kuisDone = true;
            GameManager.Instance.currMoney += 5000;
            BackToLevel.gameObject.SetActive(true);
        }
        else
        {
            feedbackTMP.text = "<color=red>Tidak lulus</color>";
            BackToLevel.gameObject.SetActive(true);
        }

    }

     void UnlockNewLevel()
    {
        if (alreadyUnlocked) return;
        alreadyUnlocked = true;

        int currentIndex = SceneManager.GetActiveScene().buildIndex;
        int unlockedLevel = PlayerPrefs.GetInt("UnlockedLevel", 1);

        int nextLevel = currentIndex++;

        if (nextLevel > unlockedLevel)
        {
            PlayerPrefs.SetInt("UnlockedLevel", nextLevel);
            PlayerPrefs.Save();
            Debug.Log("Unlocked new level: " + nextLevel);
        }
    }

    public void BackToLevelScene(int levelId)
    {
        SceneManager.LoadScene(levelId);
    }
}
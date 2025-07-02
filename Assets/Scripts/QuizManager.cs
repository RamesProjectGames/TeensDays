using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;

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

    public List<SoalData> semuaSoal = new List<SoalData>();
    private int indexSoal = 0;
    private bool answered = false;

    void Start()
    {
        LoadCSV();
        TampilkanSoal(indexSoal);
        feedbackTMP.text = "";

        for (int i = 0; i < jawabanButtons.Length; i++)
        {
            int pilihanIndex = i; // perlu closure
            jawabanButtons[i].onClick.AddListener(() => CekJawaban(pilihanIndex));
        }

        nextButton.onClick.AddListener(() =>
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
                    btn.gameObject.SetActive(false);
                nextButton.gameObject.SetActive(false);
            }
        });
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
            Debug.Log(values.Length);
            if (values.Length >= 8)
            {
                SoalData soal = new SoalData
                {
                    soal = values[1],
                    jawaban = new string[4] { values[5], values[4], values[6], values[7] },
                    kunci = values[9].ToUpper()[0] // A/B/C/D
                };
                semuaSoal.Add(soal);
            }
        }
    }

    void TampilkanSoal(int index)
    {
        ShuffleSoal();
        SoalData s = semuaSoal[index];
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
        }
        else
        {
            feedbackTMP.text = $"<color=red>Salah! Jawaban benar: {current.kunci}</color>";
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
            Debug.Log("Soal kerandom");
            int rand = Random.Range(i, semuaSoal.Count);
            var temp = semuaSoal[i];
            semuaSoal[i] = semuaSoal[rand];
            semuaSoal[rand] = temp;
        }

        if (semuaSoal.Count > 20)
        {
            semuaSoal = semuaSoal.GetRange(0, 20);
        }

    }
}
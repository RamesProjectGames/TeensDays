using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProfilManager : MonoBehaviour
{
    //public GameObject panelDaily;
    public Image profImage;
    public Image menuImage;
    public Slider expSliderProf;
    public TMP_Text currTextProf;
    public TMP_Text diaTextProf;
    public TMP_Text nameTextProf;
    public Transform changeNameProf;

    [Header("Buttons In Profile")]
    public Button[] profileBtns;
    //public Sprite[] onClickBtns;
    //public Sprite[] onUpBtns2;
    public GameObject[] kontents;
    public int selectedIndex;

    public Image sdSlider;
    public TMP_Text sdTextProgress;

    public PlayerManager playerManager;
    // Start is called before the first frame update
    void Start()
    {
        //panelDaily.SetActive(true);
        menuImage = profImage;
        expSliderProf.value = playerManager.expSlider.value;

        for (int i = 0; i < profileBtns.Length; i++)
        {
            int index = i;
            profileBtns[i].onClick.AddListener(() => OnTabClicked(index));
        }

        OnTabClicked(0); // Pilih tab pertama saat mulai

        sliderSD();
    }

    // Update is called once per frame
    void Update()
    {
        currTextProf.text = playerManager.money_text.text;
        diaTextProf.text = playerManager.diamond_text.text;
        nameTextProf.text =  (GameManager.Instance.playerData.displayName == null)? "" : GameManager.Instance.playerData.displayName;
    }
    public void OpenChangeName(bool open)
    {
        if(changeNameProf == null) return;
        if(open)
        {
            changeNameProf.LeanScale(Vector3.one, 1.5f).setOnStart(() =>
            {
                changeNameProf.transform.localScale = Vector3.zero;
            });
        }
        else
        {
            changeNameProf.LeanScale(Vector3.zero, 1.5f);
        }
    }
    public void ChangeName(string newName)
    {
        nameTextProf.text = newName;
        GameManager.Instance.playerData.displayName = newName;
        GameManager.Instance.SavePlayerDataToCloud();
    }
    void sliderSD()
    {
        int unlockedLevel = GameManager.Instance.playerData.unlockedLevel;
        int totalLevelSD = 6;

        // Clamp supaya tidak lebih dari max
        int currentLevel = Mathf.Clamp(unlockedLevel, 0, totalLevelSD);

        // Hitung progress (0 - 1)
        float progressValue = (float)currentLevel / totalLevelSD;

        // Isi slider (Image Fill)
        sdSlider.fillAmount = progressValue;

        // Text progress (pilih salah satu)
        sdTextProgress.text = currentLevel + " / " + totalLevelSD;
        // sdTextProgress.text = Mathf.RoundToInt(progressValue * 100) + "%";
    }

    public void OnTabClicked(int index)
    {
        selectedIndex = index;

        for (int i = 0; i < profileBtns.Length; i++)
        {
            Image buttonImage = profileBtns[i].GetComponent<Image>();
            TextMeshProUGUI buttonText = profileBtns[i].GetComponentInChildren<TextMeshProUGUI>();

            if (i == index)
            {
                //buttonImage.sprite = onClickBtns[i];
                kontents[i].SetActive(true);
            }
            else
            {
                //buttonImage.sprite = onUpBtns2[i];
                kontents[i].SetActive(false);
            }
        }
    }
}

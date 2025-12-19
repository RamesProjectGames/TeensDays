using System.Collections;
using System.Collections.Generic;
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

    [Header("Buttons In Profile")]
    public Button[] profileBtns;
    public Sprite[] onClickBtns;
    public Sprite[] onUpBtns2;
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
    }

    void sliderSD()
    {
        int unlockedLevel = PlayerPrefs.GetInt("UnlockedLevel", 1);

        int totalLevelSD = 6;

        float progressValue = Mathf.Clamp01((float)unlockedLevel / totalLevelSD);
        sdSlider.fillAmount = progressValue;
        sdTextProgress.text = progressValue.ToString();

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
                buttonImage.sprite = onClickBtns[i];
                kontents[i].SetActive(true);
            }
            else
            {
                buttonImage.sprite = onUpBtns2[i];
                kontents[i].SetActive(false);
            }
        }
    }
}

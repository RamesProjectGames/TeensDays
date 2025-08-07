using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProfilManager : MonoBehaviour
{

    public Image profImage;
    public Image menuImage;
    public Slider expSliderProf;
    public TMP_Text currTextProf;
    public TMP_Text diaTextProf;

    public Image sdSlider;

    public PlayerManager playerManager;
    // Start is called before the first frame update
    void Start()
    {
        menuImage = profImage;
        expSliderProf.value = playerManager.expSlider.value;
        currTextProf.text = playerManager.money_text.text;
        diaTextProf.text = playerManager.diamond_text.text;

        sliderSD();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void sliderSD()
    {
        int unlockedLevel = PlayerPrefs.GetInt("UnlockedLevel", 1);

        int totalLevelSD = 6;

        float progressValue = Mathf.Clamp01((float)unlockedLevel / totalLevelSD);
        sdSlider.fillAmount = progressValue;


    }
}

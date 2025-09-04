using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuSettings : MonoBehaviour
{
    public Slider sliderMusic, sliderBGM;
    public TextMeshProUGUI valueText, valueBGM;
    public int maxValue = 100;
    // Start is called before the first frame update
    void Start()
    {
        sliderMusic.onValueChanged.AddListener(UpdateValueText);
        sliderBGM.onValueChanged.AddListener(UpdateValueText2);

        UpdateValueText(sliderMusic.value);
        UpdateValueText2(sliderBGM.value);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void UpdateValueText(float value)
    {
        // Konversi value slider (0 - 1) ke range angka (0 - maxValue)
        int displayValue = Mathf.RoundToInt(value * maxValue);
        valueText.text = displayValue.ToString();
    }

    void UpdateValueText2(float value)
    {
        // Konversi value slider (0 - 1) ke range angka (0 - maxValue)
        int displayValue = Mathf.RoundToInt(value * maxValue);
        valueBGM.text = displayValue.ToString();
    }

    public void ChangeScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void OnApplicationQuit()
    {
        Application.Quit();
    }
}

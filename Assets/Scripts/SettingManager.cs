using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingManager : MonoBehaviour
{
    public Slider sliderMusic, sliderBGM, sliderSensitif;
    public TextMeshProUGUI valueText, valueBGM, valueSensitif;
    public int maxValue = 100;

    [Header("Invert Camera Settings")]
    public Image switchBG;
    public Sprite invertOnSprite;
    public Sprite invertOffSprite;
    public bool isOnInvert;
    public CinemachineFreeLook CinemachineFreeLook;

    [Header("Sensitivity Cam")]
    public Slider sensitivitySlider;
    public float minXSpeed = 150f;
    public float maxXSpeed = 500f;
    public TextMeshProUGUI sensitivityText;

    private void Start()
    {
        sliderMusic.onValueChanged.AddListener(UpdateValueText);
        sliderBGM.onValueChanged.AddListener(UpdateValueText2);
        sliderSensitif.onValueChanged.AddListener(UpdateValueText3);
        UpdateValueText(sliderMusic.value);
        UpdateValueText2(sliderBGM.value);
        UpdateValueText3(sliderSensitif.value);

        isOnInvert = PlayerPrefs.GetInt("InvertCamera", 1) == 0;
        UpdateSwitchUI();

        // Saat slider berubah, update sensitivity kamera
        sensitivitySlider.onValueChanged.AddListener(UpdateCameraSensitivity);

        // Set awal
        UpdateCameraSensitivity(sensitivitySlider.value);
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

    void UpdateValueText3(float value)
    {
        int displayValue = Mathf.RoundToInt(value * 10);
        valueSensitif.text = displayValue.ToString();
    }

    public void OnToggleClick()
    {
        isOnInvert = !isOnInvert; // Balik status
        PlayerPrefs.SetInt("InvertCamera", isOnInvert ? 1 : 0);
        UpdateSwitchUI();
    }

    void UpdateSwitchUI()
    {
        if (isOnInvert)
        {
            switchBG.sprite = invertOnSprite;
            CinemachineFreeLook.m_YAxis.m_InvertInput = true;
            CinemachineFreeLook.m_XAxis.m_InvertInput = true;
        }
        else
        {
            switchBG.sprite = invertOffSprite;
            CinemachineFreeLook.m_YAxis.m_InvertInput = false;
            CinemachineFreeLook.m_XAxis.m_InvertInput = false;
        }
    }

    void UpdateCameraSensitivity(float value)
    {
        if (CinemachineFreeLook != null)
        {
            // Atur speed berdasarkan slider
            CinemachineFreeLook.m_XAxis.m_MaxSpeed = Mathf.Lerp(minXSpeed, maxXSpeed, value);

            // Update teks angka
            float displayValue = Mathf.Lerp(1f, 10f, value);
            sensitivityText.text = displayValue.ToString("0.0");
        }
    }
}

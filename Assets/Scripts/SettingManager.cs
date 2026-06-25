using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
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
    [Header("Account Settings")]
    public GameObject accountPanel;
    public Transform popUpAccountPanel;
    public Sprite linkedGoogle, unlinkGoogle;
    public Image saveProgress;
    public Button loginGoogle;
private void Start()
{
    sliderMusic.onValueChanged.AddListener(UpdateValueText);
    sliderBGM.onValueChanged.AddListener(UpdateValueText2);
    sliderSensitif.onValueChanged.AddListener(UpdateValueText3);
    UpdateValueText(sliderMusic.value);
    UpdateValueText2(sliderBGM.value);
    UpdateValueText3(sliderSensitif.value);

    // Load invert setting from playerData
    if (GameManager.Instance != null && GameManager.Instance.playerData != null)
    {
        isOnInvert = GameManager.Instance.playerData.invertCamera;
    }
    else
    {
        isOnInvert = true; // Default value
    }
    UpdateSwitchUI();

    // Saat slider berubah, update sensitivity kamera
    sensitivitySlider.onValueChanged.AddListener(UpdateCameraSensitivity);

    // Set awal
    UpdateCameraSensitivity(sensitivitySlider.value);

    UpdateSavedAccount();
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
        
        // Save to playerData and cloud
        if (GameManager.Instance != null && GameManager.Instance.playerData != null)
        {
            GameManager.Instance.playerData.invertCamera = isOnInvert;
            GameManager.Instance.SavePlayerDataToCloud();
        }
        
        UpdateSwitchUI();
    }

    void UpdateSwitchUI()
    {
        if (isOnInvert)
        {
            switchBG.sprite = invertOnSprite;
            CinemachineFreeLook.m_YAxis.m_InvertInput = false;
            CinemachineFreeLook.m_XAxis.m_InvertInput = true;
        }
        else
        {
            switchBG.sprite = invertOffSprite;
            CinemachineFreeLook.m_YAxis.m_InvertInput = true;
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
    public void UpdateSavedAccount()
    {
        if(AuthenticationManager.Singleton!=null)
        {
            if(AuthenticationManager.Singleton.IsSignedInWithGoogle())
            {
                saveProgress.sprite = linkedGoogle;
                loginGoogle.interactable= false;
            }
            else
            {
                saveProgress.sprite = unlinkGoogle;
                loginGoogle.interactable = true;
            }
        }
    }
    public void LinkAccount()
    {
        if(AuthenticationManager.Singleton!=null)
        {
            AuthenticationManager.Singleton.LinkWithGoogleAsync();
        }
    }
    public void DeleteAccount()
    {
        if(AuthenticationManager.Singleton!=null)
        {
            AuthenticationManager.Singleton.DeleteAccount(() =>
            {
                SceneManager.LoadScene("MainMenu");
            });
        }
    }
    public void OpenAccountPanel(bool isOpen)
    {
        if(isOpen)
        {
            popUpAccountPanel.LeanScale(Vector3.one, 1).setOnStart(() =>
            {
                accountPanel.SetActive(true);                
            });
        }
        else
        {
            popUpAccountPanel.LeanScale(Vector3.zero, 1).setOnComplete(() =>
            {
                accountPanel.SetActive(false);
            });
        }
    }
}

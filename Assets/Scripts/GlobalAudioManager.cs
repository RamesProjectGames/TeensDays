 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class GlobalAudioManager : MonoBehaviour
{
    public AudioMixer audioMixer;
    public Slider bgmSlider;
    public Slider sfxSlider;

    void Start()
    {
        // Load saved volume from playerData or default to full (1.0)
        if (GameManager.Instance != null && GameManager.Instance.playerData != null)
        {
            bgmSlider.value = GameManager.Instance.playerData.bgmVolume;
            sfxSlider.value = GameManager.Instance.playerData.sfxVolume;
        }
        else
        {
            bgmSlider.value = 1f;
            sfxSlider.value = 1f;
        }

        SetBGMVolume(bgmSlider.value);
        SetSFXVolume(sfxSlider.value);

        // Tambahkan listener jika belum dari Inspector
        bgmSlider.onValueChanged.AddListener(SetBGMVolume);
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);
    }

    public void SetBGMVolume(float value)
    {
        audioMixer.SetFloat("BGMVolume", Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20);
        
        // Save to playerData and cloud
        if (GameManager.Instance != null && GameManager.Instance.playerData != null)
        {
            GameManager.Instance.playerData.bgmVolume = value;
            GameManager.Instance.SavePlayerDataToCloud();
        }
    }

    public void SetSFXVolume(float value)
    {
        audioMixer.SetFloat("SFXVolume", Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20);
        
        // Save to playerData and cloud
        if (GameManager.Instance != null && GameManager.Instance.playerData != null)
        {
            GameManager.Instance.playerData.sfxVolume = value;
            GameManager.Instance.SavePlayerDataToCloud();
        }
    }
}

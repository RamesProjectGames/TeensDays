using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class GlobalAudioManager : MonoBehaviour
{
    public static GlobalAudioManager Instance;

    public AudioMixer audioMixer;

    private bool isBGMMuted;
    private bool isSFXMuted;

    private void Awake()
    {
        // Singleton supaya tidak hilang saat pindah scene
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Load status mute dari PlayerPrefs
        isBGMMuted = PlayerPrefs.GetInt("BGM", 0) == 1;
        isSFXMuted = PlayerPrefs.GetInt("SFX", 0) == 1;

        ApplyVolumeSettings();
    }

    public void ToggleBGM()
    {
        isBGMMuted = !isBGMMuted;
        PlayerPrefs.SetInt("BGM", isBGMMuted ? 1 : 0);
        ApplyVolumeSettings();
    }

    public void ToggleSFX()
    {
        isSFXMuted = !isSFXMuted;
        PlayerPrefs.SetInt("SFX", isSFXMuted ? 1 : 0);
        ApplyVolumeSettings();
    }

    private void ApplyVolumeSettings()
    {
        audioMixer.SetFloat("BGMVolume", isBGMMuted ? -80f : 0f);
        audioMixer.SetFloat("SFXVolume", isSFXMuted ? -80f : 0f);
    }
}

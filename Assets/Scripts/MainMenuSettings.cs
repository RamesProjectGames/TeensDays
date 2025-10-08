using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

public class MainMenuSettings : MonoBehaviour
{
    public Slider sliderMusic, sliderBGM;
    public TextMeshProUGUI valueText, valueBGM;
    public int maxValue = 100;

    public VideoPlayer videoPlayer;
    public GameObject loadingPanel;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(PrepareAndPlay());

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

    IEnumerator PrepareAndPlay()
    {
        loadingPanel.SetActive(true);
        videoPlayer.Prepare();

        while (!videoPlayer.isPrepared)
            yield return null;

        loadingPanel.SetActive(false);
        videoPlayer.Play();
    }
}

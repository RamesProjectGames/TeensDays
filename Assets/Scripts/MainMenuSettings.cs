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
    public GameObject buttonPlayPanel;
    public GameObject loginButtonPanel;
    public Button googleLoginButton;
    public Button anonymousLoginButton;
    public Button buttonPlay;
    public Button buttonQuit;

    // Start is called before the first frame update
    void Start()
    {
        EnableLoginButtons(false);
        EnableMainButtons(false);
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
    public void EnableLoginButtons(bool enable)
    {
        googleLoginButton.interactable = enable;
        anonymousLoginButton.interactable = enable;
    }
    public void ShowPlayButton(bool show)
    {
        buttonPlayPanel.SetActive(show);
        loginButtonPanel.SetActive(!show);
    }
    public void EnableMainButtons(bool enable)
    {
        buttonPlay.interactable = enable;
        buttonQuit.interactable = enable;
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
        EnableMainButtons(true);
    }
}

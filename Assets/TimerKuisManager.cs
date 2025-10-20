using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TimerKuisManager : MonoBehaviour
{
    public Image imageTime;
    public TMP_Text timerText;
    public float timeRemaining;
    public float timeMax;

    [Header("Slider Progress")]
    public Slider quizSlider;
    public int totalQuestions;

    // Start is called before the first frame update
    void Start()
    {
        quizSlider.maxValue = totalQuestions;
        quizSlider.minValue = 1;

        UpdateSlider();
    }

    // Update is called once per frame
    void Update()
    {
        if (QuizManager.instance.indexSoal < totalQuestions)
        {
            UpdateSlider();
        }

        if (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;

            if (timeRemaining < 0)
            {
                timeRemaining = 0;
                QuizManager.instance.currHealth = 0;
                QuizManager.instance.indexSoal = 10;
                QuizManager.instance.NextButton();
                QuizManager.instance.UpdateHealthUI();
            }

            imageTime.fillAmount = timeRemaining / timeMax;

            int minutes = Mathf.FloorToInt(timeRemaining / 60f);
            int seconds = Mathf.FloorToInt(timeRemaining % 60f);

            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);

            
            
        }
    }

    void UpdateSlider()
    {
        quizSlider.value = QuizManager.instance.indexSoal;
    }
}

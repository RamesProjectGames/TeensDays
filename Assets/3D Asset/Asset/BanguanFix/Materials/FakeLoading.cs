using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FakeLoading : MonoBehaviour
{
    public Slider slider;
    public float loadingSpeed = 0.2f;
    public float stopAtProgress = 0.6f;
    private bool isPaused = false;

    void Start()
    {
        StartCoroutine(LoadProgress());
    }

    IEnumerator LoadProgress()
    {
        float progress = 0f;

        while (progress < 1f)
        {
            if (progress >= stopAtProgress && !isPaused)
            {
                isPaused = true;
                yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
            }

            if (!isPaused)
            {
                progress += Time.deltaTime * loadingSpeed;
                progress = Mathf.Clamp01(progress);
                slider.value = progress;
            }

            yield return null;
        }

        Debug.Log("Loading Selesai");
    }
}

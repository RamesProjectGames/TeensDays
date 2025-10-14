using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadProgress : MonoBehaviour
{
    public Slider slider;

    void Start()
    {
        StartCoroutine(FakeLoading());
    }

    IEnumerator FakeLoading()
    {
        float progress = 0f;
        while (progress < 1f)
        {
            progress += Time.deltaTime * 0.2f; // simulasi loading
            slider.value = progress;
            yield return null;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandphoneMamager : MonoBehaviour
{
    public RectTransform phonePanel;
    public float popDuration = 0.6f;
    public GameObject[] buttons;

    RectTransform rt;

    Vector2 normalOffsetMin;
    Vector2 normalOffsetMax;

    void Start()
    {
        rt = phonePanel;

        normalOffsetMin = rt.offsetMin;
        normalOffsetMax = rt.offsetMax;

        // Geser ke bawah dulu (off screen)
        float height = rt.rect.height * 1.2f;

        rt.offsetMin = new Vector2(normalOffsetMin.x, normalOffsetMin.y - height);
        rt.offsetMax = new Vector2(normalOffsetMax.x, normalOffsetMax.y - height);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OpenPhone()
    {
        LeanTween.value(gameObject, 0, 1, 1f)
            .setEaseOutBack()
            .setOnUpdate((float val) =>
            {
                float height = rt.rect.height * 1.2f;

                rt.offsetMin = Vector2.Lerp(
                    new Vector2(normalOffsetMin.x, normalOffsetMin.y - height),
                    normalOffsetMin,
                    val);

                rt.offsetMax = Vector2.Lerp(
                    new Vector2(normalOffsetMax.x, normalOffsetMax.y - height),
                    normalOffsetMax,
                    val);
            });
    }

    void ShowButtonsSequential()
    {
        float delay = 0f;

        foreach (GameObject btn in buttons)
        {
            btn.transform.localScale = Vector3.zero;
            btn.SetActive(true);

            LeanTween.scale(btn, Vector3.one, 0.3f)
                .setEaseOutBack()
                .setDelay(delay);

            delay += 0.1f; // delay antar tombol
        }
    }

    public void ClosePhone()
    {
        LeanTween.value(gameObject, 0, 1, 1f)
            .setEaseInBack()
            .setOnUpdate((float val) =>
            {
                float height = rt.rect.height * 1.2f;

                rt.offsetMin = Vector2.Lerp(
                    normalOffsetMin,
                    new Vector2(normalOffsetMin.x, normalOffsetMin.y - height),
                    val);

                rt.offsetMax = Vector2.Lerp(
                    normalOffsetMax,
                    new Vector2(normalOffsetMax.x, normalOffsetMax.y - height),
                    val);
            });
    }
}

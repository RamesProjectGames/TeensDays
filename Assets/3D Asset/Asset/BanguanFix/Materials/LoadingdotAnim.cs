using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingdotAnim : MonoBehaviour
{
    public TextMeshProUGUI loadingText; // Atau TextMeshProUGUI jika pakai TMP
    public float dotInterval = 0.5f;

    private float timer;
    private int dotCount = 0;
    private const int maxDots = 3;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= dotInterval)
        {
            timer = 0f;
            dotCount = (dotCount + 1) % (maxDots + 1); // 0 to 3
            loadingText.text = "Loading" + new string('.', dotCount);
        }
    }

}

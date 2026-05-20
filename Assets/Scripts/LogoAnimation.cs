using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class LogoAnimation : MonoBehaviour
{
    public RectTransform logo; // drag UI logo kamu ke sini di inspector
    public float targetY = 0f; // posisi akhir Y
    public float duration = 1f; // durasi animasi
    public UnityEvent onAnimationComplete; // event untuk dipanggil setelah animasi selesai

    void Start()
    {
        // Simpan posisi awal
        Vector3 startPos = logo.anchoredPosition;

        // Set posisi awal ke atas layar
        logo.anchoredPosition = new Vector2(startPos.x, Screen.height);

        // Mulai animasi turun + bounce
        LeanTween.moveY(logo, targetY, duration).setEase(LeanTweenType.easeOutBounce).setOnComplete(() =>
        {
            onAnimationComplete?.Invoke();
        });
    }
}

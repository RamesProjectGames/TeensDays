using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CutsceneController : MonoBehaviour
{
    [Header("Cutscene Pages")]
    public List<Image> pages;           // Drag semua Image_Page1–5 (komponen Image)
    public List<CanvasGroup> textPanels; // Drag semua Panel_Teks (tiap halaman)

    [Header("Fade Settings")]
    public Image fadeImage;             // Drag Fade Image
    public float fadeDuration = 1f;
    public GameObject panelCutscene;

    [Header("Skip Button & Tap Hint")]
    public Button skipButton;
    public GameObject tapHintPanel;

    private int currentPage = 0;
    private bool isSkipping = false;
    private bool isTapped = false;
    private bool isWaitingForTap = false;

    public float idleTimer = 0f;
    public float idleThreshold;

    private Coroutine cutsceneRoutine;

    void Start()
    {
        //panelCutscene.SetActivetrue;
        isSkipping = false;
        // Reset semua page & teks
        foreach (var img in pages)
        {
            Color c = img.color;
            c.a = 0f;
            img.color = c;
        }

        foreach (var t in textPanels)
            t.alpha = 0f;

        fadeImage.color = new Color(0, 0, 0, 1);
        skipButton.onClick.AddListener(() => SkipCutscene());

        if (tapHintPanel != null)
            tapHintPanel.SetActive(false);

        StartCoroutine(PlayCutscene());
    }

    void Update()
    {
        // Deteksi AFK hanya saat menunggu tap
        if (isWaitingForTap && !isSkipping)
        {
            idleTimer += Time.deltaTime;

            // Jika ada tap / klik
            if (Input.GetMouseButtonDown(0) || Input.touchCount > 0)
            {
                isTapped = true;
                idleTimer = 0f;
                isWaitingForTap = false;
                skipButton.gameObject.SetActive(true);

                tapHintPanel.SetActive(false);
            }

            // Jika AFK melebihi waktu threshold
            if (idleTimer >= idleThreshold)
            {
                skipButton.gameObject.SetActive(false);
                tapHintPanel.SetActive(true);
            }
        }
    }

    IEnumerator PlayCutscene()
    {
        yield return FadeOutBlack(); // Dari hitam → tampak

        while (currentPage < pages.Count && !isSkipping)
        {
            isTapped = false;

            Image currentImg = pages[currentPage];
            CanvasGroup currentText = (currentPage < textPanels.Count) ? textPanels[currentPage] : null;

            // --- Fade in gambar ---
            yield return FadeImage(currentImg, 0f, 3f);

            // --- Fade in teks ---
            if (currentText != null)
                yield return FadeCanvasGroup(currentText, 0f, 5f);

            // --- Tunggu tap dari player ---
            yield return WaitForPlayerTap();

            // --- Fade out teks ---
            if (currentText != null)
                yield return FadeCanvasGroup(currentText, 5f, 0f);

            // --- Fade out gambar ---
            yield return FadeImage(currentImg, 3f, 0f);

            currentPage++;
        }

        yield return FadeInBlack(); // Akhir cutscene: tampak → hitam

        yield return new WaitForSeconds(0.5f); // sedikit jeda agar smooth
        yield return FadeOutBlack(); // Hitam → tampak

        EndCutscene();
    }

    IEnumerator WaitForPlayerTap()
    {
        isWaitingForTap = true;
        idleTimer = 0f;

        if (tapHintPanel != null)
            tapHintPanel.SetActive(false);

        // Tunggu sampai player tap atau skip
        while (!isTapped && !isSkipping)
        {
            yield return null;
        }

        isWaitingForTap = false;
    }

    IEnumerator FadeImage(Image img, float from, float to)
    {
        float t = 0f;
        Color c = img.color;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(from, to, t / fadeDuration);
            img.color = c;
            yield return null;
        }
        c.a = to;
        img.color = c;
    }

    IEnumerator FadeCanvasGroup(CanvasGroup cg, float from, float to)
    {
        float t = 0f;
        cg.alpha = from;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(from, to, t / fadeDuration);
            yield return null;
        }
        cg.alpha = to;
    }

    IEnumerator FadeOutBlack()
    {
        float t = 0f;
        Color c = fadeImage.color;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(1f, 0f, t / fadeDuration);
            fadeImage.color = c;
            yield return null;
        }
        c.a = 0f;
        fadeImage.color = c;
    }

    IEnumerator FadeInBlack()
    {
        float t = 0f;
        Color c = fadeImage.color;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(0f, 1f, t / fadeDuration);
            fadeImage.color = c;
            yield return null;
        }
        c.a = 1f;
        fadeImage.color = c;
    }

    public void SkipCutscene()
    {
        if (isSkipping) return;

        isSkipping = true;

        // Hentikan coroutine utama jika sedang berjalan
        if (cutsceneRoutine != null)
            StopCoroutine(cutsceneRoutine);

        // Langsung mulai fade to black dan akhiri cutscene
        StartCoroutine(SkipToEnd());
    }

    IEnumerator SkipToEnd()
    {
        // Nonaktifkan semua image agar tidak tersisa di layar
        foreach (var img in pages)
        {
            Color c = img.color;
            c.a = 0f;
            img.color = c;
        }

        foreach (var t in textPanels)
            t.alpha = 0f;

        if (tapHintPanel != null)
            tapHintPanel.SetActive(false);

        yield return FadeInBlack();
        EndCutscene();
    }

    void EndCutscene()
    {
        Debug.Log("Cutscene selesai!");
        panelCutscene.SetActive(false);
    }
}
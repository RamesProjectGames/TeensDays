using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewardAnimation : MonoBehaviour
{
    public GameObject coinPrefab;   // prefab coin (UI Image)
    public Transform spawnParent;   // parent di canvas
    public Transform targetIcon;    // target icon HUD coin
    public int coinCount = 10;      // jumlah coin yang terbang
    public float spawnRadius = 100f;
    public float flyDuration = 1f;  // durasi coin terbang
    public float delayBetweenCoins = 0.05f;

    public void PlayAnimation(Vector3 startPos)
    {
        StartCoroutine(SpawnAndFlyCoins(startPos));
    }

    IEnumerator SpawnAndFlyCoins(Vector3 startPos)
    {
        for (int i = 0; i < coinCount; i++)
        {
            GameObject coin = Instantiate(coinPrefab, spawnParent);
            RectTransform rect = coin.GetComponent<RectTransform>();

            // Posisi awal coin (acak sedikit di sekitar start)
            Vector2 randomOffset = Random.insideUnitCircle * spawnRadius;
            rect.position = startPos + new Vector3(randomOffset.x, randomOffset.y, 0);

            // Jalankan animasi ke target
            StartCoroutine(FlyCoin(rect, targetIcon.position, flyDuration));

            yield return new WaitForSeconds(delayBetweenCoins);
        }
    }

    IEnumerator FlyCoin(RectTransform coin, Vector3 target, float time)
    {
        Vector3 start = coin.position;
        float elapsed = 0;

        while (elapsed < time)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / time;

            // Tambahkan kurva animasi (Easing)
            t = Mathf.Sin(t * Mathf.PI * 0.5f); // easeOutSine

            // Gerakkan coin
            coin.position = Vector3.Lerp(start, target, t);

            // Bisa kasih efek scale biar kelihatan "pop"
            coin.localScale = Vector3.Lerp(Vector3.one * 0.5f, Vector3.one, t);

            yield return null;
        }

        Destroy(coin.gameObject);

        // Tambahan: kalau coin terakhir sampai, update UI coin di HUD
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ObjectGradientController : MonoBehaviour
{
    // Properties yang sama dengan sebelumnya
    [Header("Gradient Colors")]
    public Color topColor = Color.red;
    public Color bottomColor = Color.blue;

    private Renderer objRenderer;
    private MaterialPropertyBlock propertyBlock;

    // Fungsi ini akan dipanggil di Editor saat ada perubahan di Inspector atau Scene
#if UNITY_EDITOR
    void OnValidate()
    {
        // Panggil fungsi untuk mengaplikasikan warna baru setiap kali nilai di Inspector berubah
        // Ini memastikan kita melihat hasilnya secara real-time di Editor
        InitializeAndSetColors();
    }
#endif

    // Fungsi Awake dipanggil saat memulai game (mode Play)
    void Awake()
    {
        InitializeAndSetColors();
    }

    // Fungsi utama yang menangani inisialisasi MPB dan mengatur warna
    private void InitializeAndSetColors()
    {
        objRenderer = GetComponent<Renderer>();

        if (objRenderer == null) return;

        // Inisialisasi MaterialPropertyBlock
        if (propertyBlock == null)
        {
            propertyBlock = new MaterialPropertyBlock();
        }

        // Ambil MPB yang mungkin sudah ada (penting untuk Instancing)
        objRenderer.GetPropertyBlock(propertyBlock);

        // Atur warna gradien
        propertyBlock.SetColor("_TopColor", topColor);
        propertyBlock.SetColor("_BottomColor", bottomColor);

        // Terapkan kembali MPB ke renderer objek
        objRenderer.SetPropertyBlock(propertyBlock);
    }

    // Fungsi ini akan dipanggil saat nilai di Inspector diubah dalam mode Play
    void Update()
    {
        // Jika kita berada dalam mode Play, kita ingin script Update normal tetap berjalan.
        // Jika kita mengubah nilai 'topColor' atau 'bottomColor' saat Play,
        // OnValidate tidak akan dipanggil, jadi kita panggil di sini sebagai fallback.
        if (Application.isPlaying)
        {
            // Panggil fungsi ini jika ada perubahan warna dari script lain saat runtime.
            // Namun, untuk efisiensi, lebih baik panggil SetColors hanya saat benar-benar perlu.
            // Untuk demo ini, kita biarkan saja OnValidate yang meng-handle Editor.
        }
    }
}
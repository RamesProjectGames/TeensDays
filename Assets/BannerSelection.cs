using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BannerSelection : MonoBehaviour
{
    public Vector2 offset;
    public Vector2 tiling = new Vector2(1, 0.16f); // Nilai 0.16 didapat dari 1 dibagi jumlah banner (misal 6 banner)

    private Renderer _renderer;
    private MaterialPropertyBlock _propBlock;

    void OnValidate() // Dipanggil otomatis saat Anda mengubah nilai di Inspector
    {
        UpdateBanner();
    }

    void UpdateBanner()
    {
        if (_renderer == null) _renderer = GetComponent<Renderer>();
        if (_propBlock == null) _propBlock = new MaterialPropertyBlock();

        // Angka 0 di bawah ini merujuk pada urutan material di Inspector.
        // Jika 'Banner2' ada di posisi paling atas, gunakan 0.
        // Jika 'Banner2' ada di posisi kedua, ganti menjadi 1.
        int materialIndex = 0;

        _renderer.GetPropertyBlock(_propBlock, materialIndex);

        _propBlock.SetVector("_BaseMap_ST", new Vector4(tiling.x, tiling.y, offset.x, offset.y));

        _renderer.SetPropertyBlock(_propBlock, materialIndex);
    }
}

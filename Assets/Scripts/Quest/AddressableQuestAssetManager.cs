using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AddressableQuestAssetManager : MonoBehaviour
{
    public static AddressableQuestAssetManager Instance;

    private readonly Dictionary<string, AsyncOperationHandle> downloadedAssets = new Dictionary<string, AsyncOperationHandle>();

    private void Awake()
    {
        Instance = this;
    }

    public async Task<bool> DownloadAssetAsync(AssetReference assetReference)
    {
        if (assetReference == null)
        {
            return false;
        }

        if (downloadedAssets.ContainsKey(assetReference.AssetGUID))
        {
            return true;
        }

        try
        {
            var handle = assetReference.LoadAssetAsync<GameObject>();
            await handle.Task;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                downloadedAssets[assetReference.AssetGUID] = handle;
                return true;
            }

            Debug.LogWarning($"Failed to download addressable asset '{assetReference.AssetGUID}'.");
            return false;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Addressable download failed for '{assetReference.AssetGUID}': {e.Message}");
            return false;
        }
    }

    public async Task<List<bool>> DownloadAssetsAsync(IEnumerable<AssetReference> assetReferences)
    {
        var results = new List<bool>();
        if (assetReferences == null)
        {
            return results;
        }

        foreach (var assetReference in assetReferences)
        {
            results.Add(await DownloadAssetAsync(assetReference));
        }

        return results;
    }

    public void ReleaseAsset(AssetReference assetReference)
    {
        if (assetReference == null)
        {
            return;
        }

        if (downloadedAssets.TryGetValue(assetReference.AssetGUID, out var handle))
        {
            if (handle.IsValid())
            {
                Addressables.Release(handle);
            }

            downloadedAssets.Remove(assetReference.AssetGUID);
            return;
        }

        assetReference.ReleaseAsset();
    }

    public void ReleaseAssets(IEnumerable<AssetReference> assetReferences)
    {
        if (assetReferences == null)
        {
            return;
        }

        foreach (var assetReference in assetReferences)
        {
            ReleaseAsset(assetReference);
        }
    }

    public bool IsDownloaded(AssetReference assetReference)
    {
        return assetReference != null && downloadedAssets.ContainsKey(assetReference.AssetGUID);
    }
}

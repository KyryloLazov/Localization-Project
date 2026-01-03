using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AddressablesLoader : IAddressablesLoader
{
    private readonly Dictionary<string, Object> _cache = new Dictionary<string, Object>();
    private readonly Dictionary<string, AsyncOperationHandle> _labelHandles = new Dictionary<string, AsyncOperationHandle>();
    private bool _initialized;

    public async UniTask Initialize()
    {
        if (_initialized) return;
        await Addressables.InitializeAsync().Task;
        _initialized = true;
    }

    public async UniTask<T> Load<T>(string address) where T : Object
    {
        if (_cache.TryGetValue(address, out Object existing))
            return (T)existing;

        T asset = null;
        try { asset = await Addressables.LoadAssetAsync<T>(address).Task; }
        catch (System.Exception e) { Debug.LogError("[AddressablesLoader] " + e); }

        if (asset != null && !(asset is LiveConfigSO))
            _cache[address] = asset;

        return asset;
    }

    public async UniTask Preload<T>(string address) where T : Object
    {
        if (_cache.ContainsKey(address)) return;
        await Load<T>(address);
    }

    public bool TryGetCached<T>(string address, out T asset) where T : Object
    {
        if (_cache.TryGetValue(address, out Object found))
        {
            asset = (T)found;
            return true;
        }
        asset = null;
        return false;
    }

    public void Release(string address, bool force = false)
    {
        if (_cache.TryGetValue(address, out Object found))
        {
            Addressables.Release(found);
            if (force) Resources.UnloadAsset(found);
            _cache.Remove(address);
        }
    }

    public void Clear(bool force = false)
    {
        foreach (KeyValuePair<string, Object> kv in _cache)
        {
            Addressables.Release(kv.Value);
            if (force && kv.Value != null) Resources.UnloadAsset(kv.Value);
        }
        _cache.Clear();

        foreach (KeyValuePair<string, AsyncOperationHandle> kv in _labelHandles)
        {
            Addressables.Release(kv.Value);
        }
        _labelHandles.Clear();
    }

    public async UniTask<List<T>> LoadAll<T>(string label) where T : Object
    {
        AsyncOperationHandle<IList<T>> handle = Addressables.LoadAssetsAsync<T>(label, null);
        IList<T> loaded = await handle.Task;

        if (_labelHandles.ContainsKey(label)) Addressables.Release(_labelHandles[label]);
        _labelHandles[label] = handle;

        List<T> result = new List<T>();
        if (loaded != null)
        {
            for (int i = 0; i < loaded.Count; i++)
            {
                T asset = loaded[i];
                if (asset != null) result.Add(asset);
            }
        }
        return result;
    }
}

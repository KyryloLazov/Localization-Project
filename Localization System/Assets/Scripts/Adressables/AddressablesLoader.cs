using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class AddressablesLoader : IAddressablesLoader
{
    private readonly Dictionary<string, Object> _cache = new();

    public async UniTask<T> Load<T>(string address) where T : Object
    {
        if (_cache.TryGetValue(address, out var existing))
            return (T)existing;

        T asset = null;
        try
        {
            asset = await Addressables.LoadAssetAsync<T>(address).Task;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"AddressablesLoader: failed to load {address}: {e}");
        }

        if (asset != null)
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
        if (_cache.TryGetValue(address, out var found))
        {
            asset = (T)found;
            return true;
        }
        asset = null;
        return false;
    }

    public void Release(string address)
    {
        if (_cache.TryGetValue(address, out var found))
        {
            Addressables.Release(found);
            _cache.Remove(address);
        }
    }

    public void Clear()
    {
        foreach (var kv in _cache)
            Addressables.Release(kv.Value);
        _cache.Clear();
    }
}
using Cysharp.Threading.Tasks;
using UnityEngine;

public interface IAddressablesLoader
{
    UniTask<T> Load<T>(string address) where T : Object;
    UniTask Preload<T>(string address) where T : Object;
    bool TryGetCached<T>(string address, out T asset) where T : Object;
    void Release(string address);
    void Clear();
}
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public interface IAddressablesLoader
{
    UniTask Initialize();
    UniTask<T> Load<T>(string address) where T : Object;
    UniTask Preload<T>(string address) where T : Object;
    bool TryGetCached<T>(string address, out T asset) where T : Object;
    void Release(string address, bool force = false);
    void Clear(bool force = false);
    UniTask<List<T>> LoadAll<T>(string label) where T : Object;
}
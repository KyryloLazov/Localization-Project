using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class UnityWebRequestTextProvider : IRemoteTextProvider
{
    private readonly int _timeoutSec;

    public UnityWebRequestTextProvider(int timeoutSeconds = 8)
    {
        _timeoutSec = timeoutSeconds;
    }

    private const string TAG = "[TextProvider]";

    public async UniTask<string> Fetch(string url)
    {
        using (var req = UnityWebRequest.Get(url))
        {
            req.timeout = _timeoutSec;

#if !UNITY_WEBGL || UNITY_EDITOR
            req.SetRequestHeader("Cache-Control", "no-cache, no-store, max-age=0");
            req.SetRequestHeader("Pragma", "no-cache");
            req.SetRequestHeader("If-Modified-Since", "Mon, 26 Jul 1997 05:00:00 GMT");
#endif
            try
            {
                await req.SendWebRequest();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"{TAG} EXC {e.GetType().Name}: {e.Message}");
                return null;
            }

            if (req.result != UnityWebRequest.Result.Success)
                return null;

            return req.downloadHandler.text;
        }
    }
}
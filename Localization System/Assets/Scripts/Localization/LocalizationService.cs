using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
using Zenject;

public class LocalizationService : IInitializable
{
    private const string TAG = "[Localization]";
    private const string ADDR_KEY = "LocalizationDatabase";
    private const string RES_PATH = "Localization/LocalizationDatabase";

    private readonly IRemoteTextProvider _remote;
    private readonly string _url;
    private readonly bool _useRemote;

    public LocalizationService(
        IRemoteTextProvider remote,
        [Inject(Id = "LocalizationUrl")] string url,
        [Inject(Id = "UseRemoteLocalization")] bool useRemote = true)
    {
        _remote = remote;
        _url = url;
        _useRemote = useRemote;
    }

    public async void Initialize()
    {
        try
        {
            bool localLoaded = await TryLoadLocalAsync();

            if (_useRemote)
            {
                await LoadRemote(updateExisting: localLoaded);
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"{TAG} Initialize EXC: {e}");
        }
    }

    private async UniTask<bool> TryLoadLocalAsync()
    {
        IResourceLocator initLocator = null;
        try
        {
            var init = Addressables.InitializeAsync();
            await init.Task;
            initLocator = init.Result;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"{TAG} Addressables init EXC: {e}");
        }

        bool keyLocated = false;
        try
        {
            foreach (var loc in Addressables.ResourceLocators)
            {
                if (loc.Locate(ADDR_KEY, typeof(LocalizationDatabase), out var _))
                {
                    keyLocated = true;
                    break;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"{TAG} Locate EXC: {e}");
        }

        if (keyLocated)
        {
            AsyncOperationHandle<LocalizationDatabase> handle = default;
            try
            {
                handle = Addressables.LoadAssetAsync<LocalizationDatabase>(ADDR_KEY);
                await handle.Task;
                
                if (handle.Status == AsyncOperationStatus.Succeeded && handle.Result != null)
                {
                    var dict = handle.Result.ToDictionary();
                    LogDbPreview(dict, "ADDR");
                    LocalizationManager.SetDatabase(dict);
                    return true;
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"{TAG} Addr load EXC: {e}");
            }
        }

        try
        {
            var dbSO = Resources.Load<LocalizationDatabase>(RES_PATH);
            if (dbSO != null)
            {
                var dict = dbSO.ToDictionary();
                LogDbPreview(dict, "RES");
                LocalizationManager.SetDatabase(dict);
                return true;
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"{TAG} Resources EXC: {e}");
        }

        return false;
    }

    private async UniTask LoadRemote(bool updateExisting)
    {
        if (string.IsNullOrEmpty(_url))
        {
            Debug.LogWarning($"{TAG} Remote URL is empty");
            return;
        }

        string url = UrlUtil.WithCacheBuster(_url);
        string json = await _remote.Fetch(url);

        if (string.IsNullOrWhiteSpace(json))
        {
            Debug.LogWarning($"{TAG} Remote fetch failed/null (в WebGL это часто CORS — проверь заголовки на хостинге)");
            return;
        }

        try
        {
            var db = ParseLocalizationJson(json);
            LogDbPreview(db, "REMOTE");
            LocalizationManager.SetDatabase(db);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"{TAG} Parse/apply EXC: {e}");
        }
    }

    private Dictionary<string, Dictionary<string, string>> ParseLocalizationJson(string json)
    {
        var db = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
        var root = JObject.Parse(json);

        foreach (var lang in root.Properties())
        {
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            Flatten(lang.Value, "", dict);
            db[lang.Name.ToLowerInvariant()] = dict;
        }
        return db;
    }

    private void Flatten(Newtonsoft.Json.Linq.JToken token, string prefix, Dictionary<string, string> output)
    {
        if (token is JObject obj)
        {
            foreach (var child in obj.Properties())
                Flatten(child.Value, string.IsNullOrEmpty(prefix) ? child.Name : $"{prefix}/{child.Name}", output);
        }
        else if (token is JValue val)
        {
            output[prefix] = val.ToString();
        }
    }

    private static void LogDbPreview(Dictionary<string, Dictionary<string, string>> db, string tag)
    {
        var langs = db?.Keys?.ToList() ?? new List<string>();
        var entries = db?.Sum(kv => kv.Value?.Count ?? 0) ?? 0;
    }
}

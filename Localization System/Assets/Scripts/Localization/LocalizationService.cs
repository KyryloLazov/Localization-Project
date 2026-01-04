using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Zenject;

public class LocalizationService : IInitializable
{
    private const string ADDR_KEY = "LocalizationDatabase";
    private const string RES_PATH = "Localization/LocalizationDatabase";

    private readonly IRemoteTextProvider _remote;
    private readonly LocalizationFontService _fontService;
    private readonly string _locUrl;
    private readonly string _fontsUrl;
    private readonly bool _useRemote;

    public LocalizationService(
        IRemoteTextProvider remote,
        LocalizationFontService fontService,
        [Inject(Id = "LocalizationUrl")] string locUrl,
        [Inject(Id = "FontsUrl")] string fontsUrl,
        [Inject(Id = "UseRemote")] bool useRemote)
    {
        _remote = remote;
        _fontService = fontService;
        _locUrl = locUrl;
        _fontsUrl = fontsUrl;
        _useRemote = useRemote;
    }

    public async void Initialize()
    {
        bool localLoaded = await TryLoadLocalAsync();
        Debug.Log($"[Localization] Local load result: {localLoaded}");

        if (_useRemote)
        {
            Debug.Log("[Localization] Starting remote load...");
            await UniTask.WhenAll(LoadRemoteTranslations(), LoadRemoteFonts());
        }
        
        LocalizationManager.IsInitialized = true;
    }

    private async UniTask<bool> TryLoadLocalAsync()
    {
        try
        {
            await Addressables.InitializeAsync();
            bool keyFound = false;
            foreach (var loc in Addressables.ResourceLocators)
            {
                if (loc.Locate(ADDR_KEY, typeof(LocalizationDatabase), out _)) { keyFound = true; break; }
            }

            if (keyFound)
            {
                var handle = Addressables.LoadAssetAsync<LocalizationDatabase>(ADDR_KEY);
                await handle.Task;
                if (handle.Status == AsyncOperationStatus.Succeeded && handle.Result != null)
                {
                    LocalizationManager.SetDatabase(handle.Result.ToDictionary());
                    return true;
                }
            }
        }
        catch (Exception e) { Debug.LogWarning($"[Localization] Local Addressables skip: {e.Message}"); }

        var dbSO = Resources.Load<LocalizationDatabase>(RES_PATH);
        if (dbSO != null)
        {
            LocalizationManager.SetDatabase(dbSO.ToDictionary());
            return true;
        }

        return false;
    }

    private async UniTask LoadRemoteTranslations()
    {
        if (string.IsNullOrEmpty(_locUrl)) return;
        string url = UrlUtil.WithCacheBuster(_locUrl);
        string json = await _remote.Fetch(url);

        if (string.IsNullOrWhiteSpace(json))
        {
            Debug.LogError($"[Localization] Remote JSON is empty! URL: {_locUrl}");
            return;
        }

        try
        {
            var db = ParseLocalizationJson(json);
            Debug.Log($"[Localization] Remote loaded. Languages: {db.Count}");
            if (db.Count > 0)
            {
                var firstLang = db.First().Value;
                Debug.Log($"[Localization] KEYS SAMPLE: {string.Join(", ", firstLang.Keys.Take(5))}");
            }
            LocalizationManager.SetDatabase(db);
        }
        catch (Exception e)
        {
            Debug.LogError($"[Localization] JSON Parse Error: {e}");
        }
    }

    private async UniTask LoadRemoteFonts()
    {
        if (string.IsNullOrEmpty(_fontsUrl)) return;
        string url = UrlUtil.WithCacheBuster(_fontsUrl);
        string json = await _remote.Fetch(url);

        if (!string.IsNullOrWhiteSpace(json))
        {
            try
            {
                var dict = JObject.Parse(json).ToObject<Dictionary<string, object>>();
                _fontService.UpdateConfig(dict);
            }
            catch (Exception e) { Debug.LogError($"[Localization] Fonts Config Error: {e}"); }
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

    private void Flatten(JToken token, string prefix, Dictionary<string, string> output)
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
}
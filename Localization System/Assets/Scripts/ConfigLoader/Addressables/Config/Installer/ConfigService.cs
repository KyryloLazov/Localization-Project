using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

public class ConfigService
{
    private readonly IAddressablesLoader _addr;
    private readonly IRemoteTextProvider _remote;
    private readonly LocalizationFontService _fontService;
    private readonly string _configUrl;
    private readonly string _fontsConfigUrl = "https://localization-system-4a5c3.web.app/configs/fonts.config.json";
    private readonly Dictionary<string, LiveConfigSO> _loaded = new Dictionary<string, LiveConfigSO>();

    private static bool _initStarted;
    private static bool _initCompleted;
    private static UniTaskCompletionSource _initTcs;

    public ConfigService(
        IAddressablesLoader addr,
        IRemoteTextProvider remote,
        [Inject(Id = "MainConfigUrl")] string configUrl,
        LocalizationFontService fontService)
    {
        _addr = addr;
        _remote = remote;
        _configUrl = configUrl;
        _fontService = fontService;
    }

    public async UniTask Initialize()
    {
        if (_initCompleted)
        {
            ConfigInitGate.MarkReady();
            return;
        }

        if (_initStarted)
        {
            if (_initTcs != null) await _initTcs.Task;
            return;
        }

        await ForceReload();
    }
    
    public async UniTask ForceReload()
    {
        _initCompleted = false;
        
        if (_initStarted && _initTcs != null)
        {
            await _initTcs.Task;
            return;
        }

        _initStarted = true;
        _initTcs = new UniTaskCompletionSource();

        try
        {
            Debug.Log("[ConfigService] Starting CORE reload...");
            await InitializeCore();
            
            _initCompleted = true;
            _initTcs.TrySetResult();
            Debug.Log("[ConfigService] Reload COMPLETE.");
        }
        catch (Exception ex)
        {
            _initTcs.TrySetException(ex);
            _initStarted = false;
            throw;
        }
    }

    private async UniTask InitializeCore()
    {
        _loaded.Clear();
        ConfigHub.Clear();
        await _addr.Initialize();

        IList<LiveConfigSO> configs = await _addr.LoadAll<LiveConfigSO>("Config");
        int c = configs != null ? configs.Count : 0;
        for (int i = 0; i < c; i++)
        {
            LiveConfigSO cfg = configs[i];
            if (cfg == null) continue;
            _loaded[cfg.Key] = cfg;
            ConfigHub.Set(cfg);
        }

        Dictionary<string, object> fontsJson = await FetchFlatJson(_fontsConfigUrl);
        FontsConfigSO fontsConfig = Get<FontsConfigSO>("FontsConfig");
        if (fontsConfig != null && fontsJson != null)
        {
            fontsConfig.UpdateFromRemote(fontsJson);
            _fontService.Initialize(fontsConfig);
        }

        ConfigInitGate.MarkReady();
    }

    private async UniTask<Dictionary<string, object>> FetchFlatJson(string url)
    {
        if (string.IsNullOrEmpty(url)) return null;
        string txt = await _remote.Fetch(UrlUtil.WithCacheBuster(url));
        if (string.IsNullOrEmpty(txt)) return null;
        return MiniJson.Deserialize(txt) as Dictionary<string, object>;
    }

    public IReadOnlyDictionary<string, LiveConfigSO> All() { return _loaded; }

    public T Get<T>(string key) where T : LiveConfigSO
    {
        LiveConfigSO so;
        return _loaded.TryGetValue(key, out so) ? so as T : null;
    }

    public void ReleaseAll()
    {
        _addr.Clear(true);
        ConfigHub.Clear();
        _loaded.Clear();
        Resources.UnloadUnusedAssets();
        GC.Collect();
    }
}

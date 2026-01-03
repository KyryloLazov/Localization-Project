using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using Zenject;

public sealed class LocalizationFontService : IDisposable
{
    public event Action OnFontsChanged;

    private readonly IAddressablesLoader _addr;
    private FontsConfigSO _cfg;
    private readonly Dictionary<string, TMP_FontAsset> _fontAssetCache =
        new Dictionary<string, TMP_FontAsset>(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<FontType, TMP_FontAsset> _currentFonts =
        new Dictionary<FontType, TMP_FontAsset>();

    [Inject]
    public LocalizationFontService(IAddressablesLoader addr)
    {
        _addr = addr;
    }

    public void Initialize(FontsConfigSO config)
    {
        _cfg = config;
        LocalizationManager.OnLanguageChanged += OnLanguageChanged;
        if (LocalizationManager.IsInitialized) OnLanguageChanged();
    }

    public void Dispose()
    {
        LocalizationManager.OnLanguageChanged -= OnLanguageChanged;
    }

    public TMP_FontAsset GetCurrentFont(FontType type)
    {
        _currentFonts.TryGetValue(type, out var font);
        return font;
    }

    private void OnLanguageChanged()
    {
        ApplyForLanguage(LocalizationManager.CurrentLanguage).Forget();
    }

    private async UniTaskVoid ApplyForLanguage(string lang)
    {
        _currentFonts.Clear();

        foreach (FontType fontType in Enum.GetValues(typeof(FontType)))
        {
            string fontName = _cfg.GetSpec(lang, fontType);
            if (string.IsNullOrEmpty(fontName)) continue;

            TMP_FontAsset fontAsset = await GetFont(fontName);
            if (fontAsset)
            {
                _currentFonts[fontType] = fontAsset;
            }
            else
            {
                Debug.LogWarning($"[DEBUG] Couldn't load '{fontName}'");
            }
        }

        if (_currentFonts.Count > 0) OnFontsChanged?.Invoke();
    }

    private async UniTask<TMP_FontAsset> GetFont(string fontName)
    {
        if (string.IsNullOrEmpty(fontName)) return null;

        if (_fontAssetCache.TryGetValue(fontName, out var cached))
            return cached;

        TMP_FontAsset loaded = await _addr.Load<TMP_FontAsset>(fontName);
        Debug.Log($"[DEBUG] Addressables.Load('{fontName}') -> {(loaded ? "OK" : "NULL")}");

        if (loaded == null)
        {
            try
            {
                Font osFont = Font.CreateDynamicFontFromOSFont(fontName, 16);
                if (osFont) loaded = TMP_FontAsset.CreateFontAsset(osFont);
            }
            catch (Exception e)
            {
                Debug.LogError($"Font create error {e.Message}");
            }
        }

        if (loaded)
        {
            if (loaded.material == null || loaded.material.shader == null)
            {
                var newMat = new Material(Shader.Find("TextMeshPro/Distance Field"));
                newMat.mainTexture = loaded.atlasTexture;
                loaded.material = newMat;
            }
            else
            {
                if (loaded.material.mainTexture == null && loaded.atlasTexture)
                    loaded.material.mainTexture = loaded.atlasTexture;
            }
            
            loaded.material = new Material(loaded.material);
            Shader s = Shader.Find("TextMeshPro/Distance Field");
            if (s && loaded.material.shader != s)
                loaded.material.shader = s;

            loaded.atlasPopulationMode = AtlasPopulationMode.Dynamic;
            _fontAssetCache[fontName] = loaded;
        }
        return loaded;
    }
}
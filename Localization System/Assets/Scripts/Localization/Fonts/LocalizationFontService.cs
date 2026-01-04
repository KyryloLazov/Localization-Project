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
    private readonly Dictionary<string, string> _fontConfig = new(StringComparer.OrdinalIgnoreCase);
    
    private readonly Dictionary<string, TMP_FontAsset> _fontAssetCache =
        new Dictionary<string, TMP_FontAsset>(StringComparer.OrdinalIgnoreCase);
        
    private readonly Dictionary<FontType, TMP_FontAsset> _currentFonts =
        new Dictionary<FontType, TMP_FontAsset>();

    [Inject]
    public LocalizationFontService(IAddressablesLoader addr)
    {
        _addr = addr;
        LocalizationManager.OnLanguageChanged += OnLanguageChanged;
    }

    public void UpdateConfig(Dictionary<string, object> json)
    {
        _fontConfig.Clear();
        foreach (var kvp in json)
        {
            if (kvp.Value is string v)
                _fontConfig[kvp.Key] = v;
        }
        
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
            string fontName = GetFontNameFromConfig(lang, fontType);
            if (string.IsNullOrEmpty(fontName)) continue;

            TMP_FontAsset fontAsset = await GetFont(fontName);
            if (fontAsset)
            {
                _currentFonts[fontType] = fontAsset;
            }
        }

        if (_currentFonts.Count > 0) OnFontsChanged?.Invoke();
    }

    private string GetFontNameFromConfig(string lang, FontType type)
    {
        string key = $"FontsConfig.{lang.ToUpper()}_{type.ToString()}Fonts"; 
        
        if (_fontConfig.TryGetValue(key, out string val)) return val;
        
        key = $"FontsConfig.{lang.ToUpper()}_{type.ToString()}";
        if (_fontConfig.TryGetValue(key, out val)) return val;
        
        return null;
    }

    private async UniTask<TMP_FontAsset> GetFont(string fontName)
    {
        if (string.IsNullOrEmpty(fontName)) return null;

        if (_fontAssetCache.TryGetValue(fontName, out var cached))
            return cached;

        TMP_FontAsset loaded = await _addr.Load<TMP_FontAsset>(fontName);

        if (loaded == null)
        {
            try
            {
                Font osFont = Font.CreateDynamicFontFromOSFont(fontName, 16);
                if (osFont) 
                {
                    loaded = TMP_FontAsset.CreateFontAsset(osFont);
                    loaded.name = fontName; 
                }
            }
            catch {}
        }

        if (loaded)
        {
            if (loaded.atlasPopulationMode != AtlasPopulationMode.Dynamic)
            {
                loaded.atlasPopulationMode = AtlasPopulationMode.Dynamic;
            }
            
            _fontAssetCache[fontName] = loaded;
        }
        return loaded;
    }
}
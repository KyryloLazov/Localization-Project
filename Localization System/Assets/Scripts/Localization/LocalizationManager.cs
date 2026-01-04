using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public static class LocalizationManager
{
    private const string PLAYER_PREFS_KEY = "SelectedLanguage";
    
    public static event Action OnLanguageChanged;
    public static event Action OnContentChanged;
    public static event Action OnInitialized;

    private static Dictionary<string, Dictionary<string, string>> _database;
    private static string _currentLanguage;
    private static bool _isInitialized;

    public static bool IsInitialized
    {
        get => _isInitialized;
        set 
        { 
            _isInitialized = value; 
            if(value) OnInitialized?.Invoke();
        }
    }

    public static string CurrentLanguage
    {
        get
        {
#if UNITY_EDITOR
            if (!EditorApplication.isPlaying)
            {
                var langs = GetSupportedLanguages();
                string savedLang = EditorPrefs.GetString("EditorLanguagePreview", langs.FirstOrDefault() ?? "en");
                return langs.Contains(savedLang) ? savedLang : (langs.FirstOrDefault() ?? "en");
            }
#endif
            if (string.IsNullOrEmpty(_currentLanguage))
            {
                _currentLanguage = PlayerPrefs.GetString(PLAYER_PREFS_KEY, "en");
            }
            return _currentLanguage;
        }
        set
        {
#if UNITY_EDITOR
            if (!EditorApplication.isPlaying)
            {
                EditorPrefs.SetString("EditorLanguagePreview", value.ToLowerInvariant());
                OnLanguageChanged?.Invoke();
                return;
            }
#endif
            var lower = value.ToLowerInvariant();
            if (_currentLanguage == lower) return;

            _currentLanguage = lower;
            PlayerPrefs.SetString(PLAYER_PREFS_KEY, lower);
            PlayerPrefs.Save();
            OnLanguageChanged?.Invoke();
        }
    }

    public static void SetDatabase(Dictionary<string, Dictionary<string, string>> db)
    {
        _database = db ?? new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);
        
        OnContentChanged?.Invoke();

        var langs = GetSupportedLanguages();
        string current = CurrentLanguage;
        
        if (!langs.Contains(current, StringComparer.OrdinalIgnoreCase) && langs.Count > 0)
        {
            _currentLanguage = langs[0].ToLowerInvariant();
            PlayerPrefs.SetString(PLAYER_PREFS_KEY, _currentLanguage);
        }
        
        OnLanguageChanged?.Invoke();
    }

    public static List<string> GetSupportedLanguages()
    {
        if (_database == null) return new List<string>();
        return _database.Keys.ToList();
    }

#if UNITY_EDITOR
    public static IEnumerable<string> GetAllLocalizationKeys()
    {
        var dbSO = AssetDatabase.LoadAssetAtPath<LocalizationDatabase>("Assets/Data/Localization/LocalizationDatabase.asset");
        if (dbSO != null)
        {
            var dbDict = dbSO.ToDictionary();
            return dbDict.Values.SelectMany(langMap => langMap.Keys).Distinct().ToList();
        }
        return Enumerable.Empty<string>();
    }
    
    public static void InitializeForEditor()
    {
         var dbSO = AssetDatabase.LoadAssetAtPath<LocalizationDatabase>("Assets/Data/Localization/LocalizationDatabase.asset");
        if (dbSO == null)
            dbSO = Resources.Load<LocalizationDatabase>("Localization/LocalizationDatabase");

        if (dbSO != null)
        {
            SetDatabase(dbSO.ToDictionary());
            _isInitialized = true;
        }
    }
#endif

    public static string Get(string key, params object[] args)
    {
        if (string.IsNullOrWhiteSpace(key)) return "";
        if (_database == null) return $"[{key}]";

        var lang = CurrentLanguage.ToLowerInvariant();
        
        if (!_database.TryGetValue(lang, out var map) || !map.TryGetValue(key, out var template))
        {
            return $"[{key}]";
        }

        if (args is { Length: > 0 })
        {
            try { return string.Format(template, args); }
            catch { return $"[FORMAT_ERR:{key}]"; }
        }

        return template;
    }
}
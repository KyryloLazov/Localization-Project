using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


public static class LocalizationManager
{
    public static event Action OnLanguageChanged;

    private const string DatabasePath = "LocalizationDatabase";
    private const string LanguagePlayerPrefsKey = "SelectedLanguage";

    private static LocalizationDatabase _database;
    private static string _currentLanguage;

    public static string CurrentLanguage
    {
        get
        {
#if UNITY_EDITOR
            if (!EditorApplication.isPlaying)
            {
                return EditorPrefs.GetString("EditorLanguage", GetDefaultLanguage());
            }
#endif
            return _currentLanguage;
        }
        set
        {
#if UNITY_EDITOR
            if (!EditorApplication.isPlaying)
            {
                EditorPrefs.SetString("EditorLanguage", value);
                OnLanguageChanged?.Invoke();
                return;
            }
#endif
            if (_currentLanguage == value) return;

            _currentLanguage = value;
            PlayerPrefs.SetString(LanguagePlayerPrefsKey, _currentLanguage);
            PlayerPrefs.Save();
            OnLanguageChanged?.Invoke();
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        LoadDatabase();
        if (_database == null) return;

        _currentLanguage = PlayerPrefs.GetString(LanguagePlayerPrefsKey, GetDefaultLanguage());
    }

    private static void LoadDatabase()
    {
        if (_database == null)
        {
            _database = Resources.Load<LocalizationDatabase>(DatabasePath);
        }
    }
    
#if UNITY_EDITOR
    public static IEnumerable<string> GetAllLocalizationKeys()
    {
        LoadDatabase();
        if (_database != null)
        {
            return _database.GetAllKeys();
        }
        return new List<string>();
    }
#endif

    public static string Get(string key, params object[] args)
    {
        LoadDatabase();
        if (_database == null)
        {
            return $"[NO_DB:{key}]";
        }

        string format = _database.GetText(key, CurrentLanguage);
        if (args == null || args.Length == 0)
        {
            return format;
        }

        try
        {
            return string.Format(format, args);
        }
        catch (FormatException)
        {
            return $"[FORMAT_ERR:{key}]";
        }
    }

    public static List<string> GetSupportedLanguages()
    {
        LoadDatabase();
        return _database != null ? _database.GetSupportedLanguages() : new List<string>();
    }

    private static string GetDefaultLanguage()
    {
        LoadDatabase();
        var languages = GetSupportedLanguages();
        return languages.Count > 0 ? languages[0] : "en";
    }

#if UNITY_EDITOR
    public static void ForceRefresh()
    {
        OnLanguageChanged?.Invoke();
    }
#endif
}

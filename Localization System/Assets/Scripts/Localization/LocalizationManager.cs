using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System;

public static class LocalizationManager
{
    public static event Action OnLanguageChanged;
    
    private const string LanguagePlayerPrefsKey = "SelectedLanguage";
    private const string DATABASE_ADDRESS = "LocalizationData";
    
    private static LocalizationDatabase _database;
    private static string _currentLanguage;
    private static bool _isInitialized = false;
    
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static async void Initialize()
    {
        await UpdateAndLoadDatabaseAsync();

        if (_database != null)
        {
            string defaultLanguage = GetDefaultLanguage();
            CurrentLanguage = PlayerPrefs.GetString(LanguagePlayerPrefsKey, defaultLanguage);
        }
        
        _isInitialized = true;
        OnLanguageChanged?.Invoke();
    }
    
    public static string CurrentLanguage
    {
        get => _currentLanguage;
        set
        {
            if (_currentLanguage == value) return;
            
            _currentLanguage = value;
            PlayerPrefs.SetString(LanguagePlayerPrefsKey, _currentLanguage);
            PlayerPrefs.Save();
            OnLanguageChanged?.Invoke();
        }
    }

    private static async UniTask<bool> UpdateAndLoadDatabaseAsync()
    {
        var checkHandle = Addressables.CheckForCatalogUpdates(false);
        await checkHandle.Task;

        if (checkHandle.Status == AsyncOperationStatus.Succeeded && checkHandle.Result.Count > 0)
        {
            Debug.Log("New localization catalog found, downloading updates...");
            var updateHandle = Addressables.UpdateCatalogs(checkHandle.Result, false);
            await updateHandle.Task;
            Addressables.Release(updateHandle);
        }
        Addressables.Release(checkHandle);
        
        Debug.Log("Loading localization database via Addressables...");
        _database = await Services.Loader.Load<LocalizationDatabase>(DATABASE_ADDRESS);
        
        return _database != null;
    }
    
    public static string Get(string key, params object[] args)
    {
        if (!_isInitialized || _database == null)
        {
            return $"[...]";
        }

        string format = _database.GetText(key, CurrentLanguage);
        if (args == null || args.Length == 0) return format;

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
        return _database != null ? _database.GetSupportedLanguages() : new List<string>();
    }
    
    private static string GetDefaultLanguage()
    {
        var languages = GetSupportedLanguages();
        return languages.Count > 0 ? languages[0] : "en";
    }

#if UNITY_EDITOR
    public static IEnumerable<string> GetAllLocalizationKeys()
    {
        var db = UnityEditor.AssetDatabase.LoadAssetAtPath<LocalizationDatabase>("Assets/Resources_moved/LocalizationDatabase.asset");
        return db != null ? db.GetAllKeys() : new List<string>();
    }
    
    public static void ForceRefresh() => OnLanguageChanged?.Invoke();
#endif
}
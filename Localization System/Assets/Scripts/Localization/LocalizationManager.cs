using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System;
using UnityEditor;

public static class LocalizationManager
{
    public static event Action OnLanguageChanged;
    
    private const string LanguagePlayerPrefsKey = "SelectedLanguage";
    private const string DATABASE_ADDRESS = "LocalizationData";
    
    private static LocalizationDatabase _database;
    private static string _currentLanguage;
    private static bool _isInitialized = false;
    public static bool IsInitialized => _isInitialized;
    
#if UNITY_EDITOR
    static LocalizationManager()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        if (!EditorApplication.isPlayingOrWillChangePlaymode)
        {
            InitializeForEditor();
        }
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode)
        {
            _database = null;
            _isInitialized = false;
        }
        if (state == PlayModeStateChange.EnteredEditMode)
        {
            InitializeForEditor();
        }
    }

    private static void InitializeForEditor()
    {
        Debug.Log("Initializing LocalizationManager for Editor...");
        _database = AssetDatabase.LoadAssetAtPath<LocalizationDatabase>("Assets/Data/Localization/LocalizationDatabase.asset");
        if (_database != null)
        {
            CurrentLanguage = EditorPrefs.GetString("EditorLanguage", GetDefaultLanguage());
            _isInitialized = true;
            OnLanguageChanged?.Invoke();
        }
    }
#endif
    
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    public static async void Initialize()
    {
        // Цей метод тепер буде працювати тільки для Play Mode та білдів
        if (_isInitialized) return;

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
        get
        {
#if UNITY_EDITOR
            if (!EditorApplication.isPlaying)
            {
                return EditorPrefs.GetString("EditorLanguage", "en");
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
            PlayerPrefs.SetString(LanguagePlayerPrefsKey, value);
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
#if UNITY_EDITOR
        if (!EditorApplication.isPlaying && !_isInitialized)
        {
            InitializeForEditor();
        }
#endif
        if (string.IsNullOrWhiteSpace(key))
            return "";
        
        if (!_isInitialized)
        {
            return $"Not initialized";
        }

        if (_database == null)
        {
            return $"data base {DATABASE_ADDRESS} not found";
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
        var db = UnityEditor.AssetDatabase.LoadAssetAtPath<LocalizationDatabase>("Assets/Data/Localization/LocalizationDatabase.asset");
        return db != null ? db.GetAllKeys() : new List<string>();
    }
    
    public static void ForceRefresh() => OnLanguageChanged?.Invoke();
#endif
}
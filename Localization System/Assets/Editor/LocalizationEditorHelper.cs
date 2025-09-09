#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class LocalizationEditorHelper
{
    private const string DATABASE_PATH = "Assets/Resources/LocalizationDatabase.asset";
    private static LocalizationDatabase _cachedDatabase;

    public static LocalizationDatabase GetDatabase()
    {
        if (_cachedDatabase != null)
        {
            return _cachedDatabase;
        }

        _cachedDatabase = AssetDatabase.LoadAssetAtPath<LocalizationDatabase>(DATABASE_PATH);

        if (_cachedDatabase == null)
        {
            _cachedDatabase = Resources.Load<LocalizationDatabase>("LocalizationDatabase");
        }

        return _cachedDatabase;
    }
}

#endif
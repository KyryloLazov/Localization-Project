#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

public static class LocalizationImporter
{
    private const string EDITOR_DB_PATH  = "Assets/Data/Localization/LocalizationDatabase.asset";
    private const string RUNTIME_DB_PATH = "Assets/Resources/Localization/LocalizationDatabase.asset";
    private const string REMOTE_URL      = "https://testconfigs-c3b83.web.app/localization/localization.config.json";

    [MenuItem("Tools/Localization/Update From Remote (Data+Resources)")]
    public static async void UpdateLocalDatabase()
    {
        Debug.Log("[LocalizationImporter] Fetching remote localization…");

        using var req = UnityWebRequest.Get(UrlUtil.WithCacheBuster(REMOTE_URL));
        await req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"[LocalizationImporter] Download error: {req.error}");
            return;
        }

        var json = req.downloadHandler.text;
        Dictionary<string, Dictionary<string, string>> db;
        try
        {
            db = ParseJson(json);
        }
        catch (Exception e)
        {
            Debug.LogError($"[LocalizationImporter] JSON parse error: {e}");
            return;
        }
        
        var editorSo = LoadOrCreate(EDITOR_DB_PATH);
        editorSo.UpdateFrom(db);
        EditorUtility.SetDirty(editorSo);
        
        var runtimeSo = LoadOrCreate(RUNTIME_DB_PATH);
        runtimeSo.UpdateFrom(db);
        EditorUtility.SetDirty(runtimeSo);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[LocalizationImporter] ✅ Updated {db.Count} languages →\n" +
                  $"    {EDITOR_DB_PATH}\n" +
                  $"    {RUNTIME_DB_PATH}");
    }
    
    [MenuItem("Tools/Localization/Copy Data → Resources (no download)")]
    public static void CopyEditorToResources()
    {
        var editorSo = AssetDatabase.LoadAssetAtPath<LocalizationDatabase>(EDITOR_DB_PATH);
        if (editorSo == null)
        {
            Debug.LogError($"[LocalizationImporter] Not found: {EDITOR_DB_PATH}");
            return;
        }

        var runtimeSo = LoadOrCreate(RUNTIME_DB_PATH);
        runtimeSo.UpdateFrom(editorSo.ToDictionary());
        EditorUtility.SetDirty(runtimeSo);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[LocalizationImporter] ✅ Copied Data → Resources:\n    {RUNTIME_DB_PATH}");
    }

    private static LocalizationDatabase LoadOrCreate(string assetPath)
    {
        var so = AssetDatabase.LoadAssetAtPath<LocalizationDatabase>(assetPath);
        if (so != null) return so;

        EnsureDir(assetPath);
        so = ScriptableObject.CreateInstance<LocalizationDatabase>();
        AssetDatabase.CreateAsset(so, assetPath);
        return so;
    }

    private static void EnsureDir(string assetPath)
    {
        var dir = Path.GetDirectoryName(assetPath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);
    }

    private static Dictionary<string, Dictionary<string, string>> ParseJson(string json)
    {
        var root = JObject.Parse(json);
        var db = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);

        foreach (var langProp in root.Properties())
        {
            if (langProp.Value is not JObject langObj) continue;

            var flat = new Dictionary<string, string>();
            Flatten(langObj, "", flat);
            db[langProp.Name.ToLowerInvariant()] = flat;
        }
        return db;
    }

    private static void Flatten(JToken token, string prefix, Dictionary<string, string> output)
    {
        switch (token)
        {
            case JObject obj:
                foreach (var p in obj.Properties())
                {
                    var full = string.IsNullOrEmpty(prefix) ? p.Name : $"{prefix}/{p.Name}";
                    Flatten(p.Value, full, output);
                }
                break;
            case JArray arr:
                for (int i = 0; i < arr.Count; i++)
                    Flatten(arr[i], $"{prefix}[{i}]", output);
                break;
            case JValue val:
                output[prefix] = val.ToString();
                break;
        }
    }
}
#endif

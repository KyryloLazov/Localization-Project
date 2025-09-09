#if UNITY_EDITOR
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;


public class LocalizationImporterRunner
{
    private const string DATABASE_PATH = "Assets/Resources/LocalizationDatabase.asset";

    [MenuItem("Tools/Localization/Import from Google Sheet", false, 0)]
    public static async UniTask RunImport()
    {
        const string sheetId = "1rWdNUVVotSquMQmITHu4romDAhRLvPQjuAlc6P0slf0";
        const string gid = "0#gid=0";

        if (sheetId == "YOUR_SHEET_ID" || gid == "YOUR_GID")
        {
            Debug.LogError("Please set your Google Sheet ID and GID in LocalizationImporterRunner.cs");
            return;
        }

        string url = $"https://docs.google.com/spreadsheets/d/{sheetId}/export?format=csv&gid={gid}";
            
        LocalizationImporter importer = new LocalizationImporter(url);
        var data = await importer.ImportAsync();

        if (data == null || data.Count == 0)
        {
            Debug.LogWarning("[Localization Importer] No data was imported.");
            return;
        }
            
        var database = AssetDatabase.LoadAssetAtPath<LocalizationDatabase>(DATABASE_PATH);
        if (database == null)
        {
            database = ScriptableObject.CreateInstance<LocalizationDatabase>();
            AssetDatabase.CreateAsset(database, DATABASE_PATH);
        }
            
        database.UpdateData(data);
            
        EditorUtility.SetDirty(database);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[Localization Importer] Import complete. {data.Count} keys updated.");
    }
}
#endif
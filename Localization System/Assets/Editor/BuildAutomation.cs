#if UNITY_EDITOR
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

public class BuildAutomation
{
    // Цей атрибут створює пункт меню в редакторі Unity
    [MenuItem("Tools/Build/Build Localization For Server")]
    public static void BuildLocalization()
    {
        Debug.Log("--- Starting Localization Build ---");

        // Крок 1: Запускаємо імпорт з Google Sheets і чекаємо його завершення
        Debug.Log("Step 1/3: Importing data from Google Sheet...");
        LocalizationImporterRunner.RunImport().AsTask().Wait();
        Debug.Log("Import finished successfully.");

        // Крок 2: Встановлюємо профіль 'Production', щоб Addressables зібралися для сервера
        Debug.Log("Step 2/3: Setting Addressables profile to 'Production'...");
        string profileId = AddressableAssetSettingsDefaultObject.Settings.profileSettings.GetProfileId("Production");
        if (string.IsNullOrEmpty(profileId))
        {
            Debug.LogError("Addressables profile 'Production' not found! Please create it in the Addressables Profiles window.");
            return;
        }
        AddressableAssetSettingsDefaultObject.Settings.activeProfileId = profileId;
        Debug.Log("Profile set.");

        // Крок 3: Збираємо Addressables. Результат потрапить у папку ServerData
        Debug.Log("Step 3/3: Building Addressables content...");
        AddressableAssetSettings.CleanPlayerContent();
        AddressableAssetSettings.BuildPlayerContent();
        
        Debug.Log("--- Localization Build Complete! ---");
        Debug.Log("New content is ready in the 'ServerData' folder. You can now commit and push it to the repository.");
        EditorUtility.DisplayDialog("Build Complete", 
            "Localization content has been successfully built into the 'ServerData' folder.", "OK");
    }
}
#endif
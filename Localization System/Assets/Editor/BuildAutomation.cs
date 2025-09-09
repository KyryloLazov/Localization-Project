#if UNITY_EDITOR
using Cysharp.Threading.Tasks; // <-- Додайте цей using
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

public class BuildAutomation
{
    [MenuItem("Tools/Build/Build Localization For Server")]
    public static void BuildLocalization()
    {
        Debug.Log("Starting localization import...");
        
        // Правильний спосіб дочекатися завершення UniTask в синхронному методі
        LocalizationImporterRunner.RunImport().AsTask().Wait();
        
        Debug.Log("Import finished.");

        Debug.Log("Building Addressables...");
        string profileId = AddressableAssetSettingsDefaultObject.Settings.profileSettings.GetProfileId("Production");
        if (string.IsNullOrEmpty(profileId))
        {
            Debug.LogError("Addressables profile 'Production' not found! Please create it.");
            return;
        }
        
        AddressableAssetSettingsDefaultObject.Settings.activeProfileId = profileId;
        
        AddressableAssetSettings.CleanPlayerContent();
        AddressableAssetSettings.BuildPlayerContent();
        Debug.Log("Addressables build complete!");
    }
}
#endif
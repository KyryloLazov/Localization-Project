#if UNITY_EDITOR
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

public class BuildAutomation
{
    // Метод тепер є "async void"
    [MenuItem("Tools/Build/Build Localization For Server")]
    public static async void BuildLocalization()
    {
        Debug.Log("--- Starting Localization Build ---");

        try
        {
            // Крок 1: Тепер ми використовуємо неблокуючий "await"
            Debug.Log("Step 1/3: Importing data from Google Sheet...");
            await LocalizationImporterRunner.RunImport();
            Debug.Log("Import finished successfully.");

            // Крок 2: Встановлюємо профіль 'Production'
            Debug.Log("Step 2/3: Setting Addressables profile to 'Production'...");
            string profileId = AddressableAssetSettingsDefaultObject.Settings.profileSettings.GetProfileId("Production");
            if (string.IsNullOrEmpty(profileId))
            {
                Debug.LogError("Addressables profile 'Production' not found! Please create it in the Addressables Profiles window.");
                return;
            }
            AddressableAssetSettingsDefaultObject.Settings.activeProfileId = profileId;
            Debug.Log("Profile set.");

            // Крок 3: Збираємо Addressables
            Debug.Log("Step 3/3: Building Addressables content...");
            AddressableAssetSettings.CleanPlayerContent();
            AddressableAssetSettings.BuildPlayerContent();
            
            Debug.Log("--- Localization Build Complete! ---");
            Debug.Log("New content is ready in the 'ServerData' folder. You can now commit and push it to the repository.");
            EditorUtility.DisplayDialog("Build Complete", 
                "Localization content has been successfully built into the 'ServerData' folder.", "OK");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Localization build failed: {e.Message}\n{e.StackTrace}");
            EditorUtility.DisplayDialog("Build Failed", 
                $"An error occurred during the build process. Check the console for details.", "OK");
        }
    }
}
#endif
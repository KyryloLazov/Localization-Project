#if UNITY_EDITOR
using UnityEditor;
using UnityEngine; // Потрібен для Debug.Log

namespace Core.Localization.Editor
{
    public static class LocalizationMenu
    {
        [MenuItem("Tools/Localization/Preview Language", false, 11)]
        private static void SelectLanguage()
        {
            LanguageSelectorWindow.ShowWindow();
        }
        
        // [MenuItem("Tools/Localization/Force Refresh Preview", false, 12)]
        // private static void RefreshPreview()
        // {
        //     Debug.Log("Force Refresh Preview was triggered!");
        //     LocalizationManager.ForceRefresh();
        // }
    }
}
#endif
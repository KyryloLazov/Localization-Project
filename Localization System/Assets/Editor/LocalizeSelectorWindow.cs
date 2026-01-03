#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Core.Localization.Editor
{
    public class LanguageSelectorWindow : EditorWindow
    {
        public static void ShowWindow()
        {
            GetWindow<LanguageSelectorWindow>(true, "Select Preview Language", true);
        }

        private void OnGUI()
        {
            LocalizationManager.InitializeForEditor(); 

            if (!LocalizationManager.IsInitialized)
            {
                EditorGUILayout.HelpBox("Localization Manager is not initialized. Please ensure LocalizationDatabase.asset exists and is correctly structured.", MessageType.Warning);
                return;
            }

            List<string> languages = LocalizationManager.GetSupportedLanguages();
            if (languages.Count == 0)
            {
                EditorGUILayout.HelpBox("No languages found in the database. Please update local database from remote.", MessageType.Info);
                return;
            }

            EditorGUILayout.LabelField("Please select a language for preview:");
            
            foreach (string lang in languages)
            {
                bool isCurrent = (LocalizationManager.CurrentLanguage == lang);
                if (GUILayout.Toggle(isCurrent, lang, "Button"))
                {
                    if (!isCurrent)
                    {
                        LocalizationManager.CurrentLanguage = lang;
                        Debug.Log($"Preview language set to: {lang}");
                        Close(); 
                    }
                }
            }
        }
    }
}
#endif
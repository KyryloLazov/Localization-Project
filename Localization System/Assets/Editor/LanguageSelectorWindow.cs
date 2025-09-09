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
            var database = LocalizationEditorHelper.GetDatabase();
            if (database == null)
            {
                EditorGUILayout.LabelField("Database not found! Please run the importer.");
                return;
            }

            List<string> languages = database.GetSupportedLanguages();
            if (languages.Count == 0)
            {
                EditorGUILayout.LabelField("No languages found in the database!");
                return;
            }

            EditorGUILayout.LabelField("Please select a language for preview:");
            
            foreach (string lang in languages)
            {
                if (GUILayout.Button(lang))
                {
                    LocalizationManager.CurrentLanguage = lang;
                    Debug.Log($"Preview language set to: {lang}");
                    this.Close();
                }
            }
        }
    }
}
#endif
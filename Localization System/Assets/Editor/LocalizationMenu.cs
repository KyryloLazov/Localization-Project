#if UNITY_EDITOR
using UnityEditor;

namespace Core.Localization.Editor
{
    public static class LocalizationMenu
    {
        [MenuItem("Tools/Localization/Preview Language", false, 11)]
        private static void SelectLanguage()
        {
            LanguageSelectorWindow.ShowWindow();
        }
    }
}
#endif
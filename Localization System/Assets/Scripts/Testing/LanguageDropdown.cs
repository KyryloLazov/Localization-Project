using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LanguageDropdown : MonoBehaviour
{
   private TMP_Dropdown _dropdown;
    void Start()
    {
        _dropdown = GetComponent<TMP_Dropdown>();
        
        List<string> languageNames = new List<string>();
        var languages = LocalizationManager.GetSupportedLanguages();

        foreach (var language in languages)
        {
            languageNames.Add(language);
        }
        _dropdown.ClearOptions();
        _dropdown.AddOptions(languageNames);
        
        LoadLanguage(languages);

        _dropdown.onValueChanged.AddListener(OnLanguageDropdownValueChanged);
    }

    private void LoadLanguage(List<string> languages)
    {
        string savedLanguage = PlayerPrefs.GetString("SelectedLanguage", languages[0]);
        int languageIndex = languages.IndexOf(savedLanguage);
        if (languageIndex < 0) languageIndex = 0;

        _dropdown.value = languageIndex;
        _dropdown.RefreshShownValue();

        LocalizationManager.CurrentLanguage = languages[languageIndex];
    }

    private void OnLanguageDropdownValueChanged(int value)
    {
        LocalizationManager.CurrentLanguage = _dropdown.options[value].text;
    }
}

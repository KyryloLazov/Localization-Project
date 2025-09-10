using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LanguageDropdown : MonoBehaviour
{
    private TMP_Dropdown _dropdown;
    void Start()
    {
        _dropdown = GetComponent<TMP_Dropdown>();
        _dropdown.onValueChanged.AddListener(OnLanguageDropdownValueChanged);
        LocalizationManager.OnLanguageChanged += RefreshDropdownSelection;
        
        RefreshDropdownOptions();
    }

    private void OnDestroy()
    {
        LocalizationManager.OnLanguageChanged -= RefreshDropdownSelection;
    }

    private void RefreshDropdownOptions()
    {
        var languages = LocalizationManager.GetSupportedLanguages();
        if (languages.Count == 0)
        {
            Debug.Log("No languages available");
            return;
        }

        _dropdown.ClearOptions();
        _dropdown.AddOptions(languages);

        RefreshDropdownSelection();
    }

    private void RefreshDropdownSelection()
    {
        var languages = LocalizationManager.GetSupportedLanguages();
        if (languages.Count == 0) return;

        string currentLanguage = LocalizationManager.CurrentLanguage;
        if (string.IsNullOrEmpty(currentLanguage))
        {
            currentLanguage = PlayerPrefs.GetString("SelectedLanguage", languages[0]);
        }
        
        int languageIndex = languages.IndexOf(currentLanguage);
        if (languageIndex < 0) languageIndex = 0;

        _dropdown.SetValueWithoutNotify(languageIndex);
    }
    
    private void OnLanguageDropdownValueChanged(int value)
    {
        var languages = LocalizationManager.GetSupportedLanguages();
        if (value >= 0 && value < languages.Count)
        {
            LocalizationManager.CurrentLanguage = languages[value];
        }
    }
}
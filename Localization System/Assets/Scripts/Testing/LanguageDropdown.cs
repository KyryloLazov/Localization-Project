using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks; // <-- Додайте using
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
        
        DelayedInitialization().Forget();
    }
    
    private async UniTask DelayedInitialization()
    {
        await UniTask.WaitForSeconds(0.5f); 
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
            Debug.LogWarning("LanguageDropdown: No languages available from LocalizationManager.");
            return;
        }
        
        _dropdown.ClearOptions();
        
        var options = languages.Select(lang => new TMP_Dropdown.OptionData(lang)).ToList();
        _dropdown.options = options;

        RefreshDropdownSelection();
    }

    private void RefreshDropdownSelection()
    {
        var languages = LocalizationManager.GetSupportedLanguages();
        if (languages.Count == 0) return;

        string currentLanguage = LocalizationManager.CurrentLanguage;
        
        int languageIndex = languages.IndexOf(currentLanguage);
        if (languageIndex < 0)
        {
            languageIndex = 0;
            LocalizationManager.CurrentLanguage = languages[languageIndex];
        }

        _dropdown.SetValueWithoutNotify(languageIndex);
    }
    
    private void OnLanguageDropdownValueChanged(int value)
    {
        if (value >= 0 && value < _dropdown.options.Count)
        {
            LocalizationManager.CurrentLanguage = _dropdown.options[value].text;
        }
    }
}
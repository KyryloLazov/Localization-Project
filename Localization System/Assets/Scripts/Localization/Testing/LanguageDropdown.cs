using System.Linq;
using TMPro;
using UnityEngine;

public class LanguageDropdown : MonoBehaviour
{
    [SerializeField] private bool _showPlaceholder = false;

    private TMP_Dropdown _dropdown;
    private bool _placeholderActive;
    
    private void Awake()
    {
        _dropdown = GetComponent<TMP_Dropdown>();
        _dropdown.onValueChanged.AddListener(OnLanguageDropdownValueChanged);
        
        LocalizationManager.OnLanguageChanged += RefreshDropdownSelection;
        LocalizationManager.OnContentChanged += RefreshDropdownOptions;
    }

    private void Start()
    {
        RefreshDropdownOptions();
    }

    private void OnDestroy()
    {
        LocalizationManager.OnLanguageChanged -= RefreshDropdownSelection;
        LocalizationManager.OnContentChanged -= RefreshDropdownOptions;
    }

    private void RefreshDropdownOptions()
    {
        var languages = LocalizationManager.GetSupportedLanguages();
        if (languages.Count == 0) return;

        _dropdown.ClearOptions();

        if (_showPlaceholder)
        {
            _dropdown.options.Add(new TMP_Dropdown.OptionData("--Language--"));
            _placeholderActive = true;
        }
        else
        {
            _placeholderActive = false;
        }
        
        var options = languages.Select(l => 
        {
            string label = char.ToUpper(l[0]) + l.Substring(1);
            return new TMP_Dropdown.OptionData(label);
        }).ToList();
        
        _dropdown.AddOptions(options);
        
        RefreshDropdownSelection();
    }

    private int GetCurrentLanguageIndex(System.Collections.Generic.List<string> languages)
    {
        string current = LocalizationManager.CurrentLanguage;
        int index = languages.FindIndex(l => l.Equals(current, System.StringComparison.OrdinalIgnoreCase));
        return index < 0 ? 0 : index;
    }

    private void RefreshDropdownSelection()
    {
        var languages = LocalizationManager.GetSupportedLanguages();
        if (languages.Count == 0) return;

        int index = GetCurrentLanguageIndex(languages);
        
        if (_placeholderActive)
        {
            _dropdown.SetValueWithoutNotify(index + 1);
        }
        else
        {
            _dropdown.SetValueWithoutNotify(index);
        }
        
        _dropdown.RefreshShownValue();
    }

    private void OnLanguageDropdownValueChanged(int value)
    {
        if (_placeholderActive && value == 0) return;

        int realIndex = _placeholderActive ? value - 1 : value;
        var languages = LocalizationManager.GetSupportedLanguages();

        if (realIndex >= 0 && realIndex < languages.Count)
        {
            string langCode = languages[realIndex];
            LocalizationManager.CurrentLanguage = langCode;
            
            if (_placeholderActive)
            {
                _placeholderActive = false;
                RefreshDropdownOptions(); 
            }
        }
    }
}
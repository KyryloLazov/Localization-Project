using System.Linq;
using TMPro;
using UnityEngine;

public class LanguageDropdown : MonoBehaviour
{
    [SerializeField] private bool _showPlaceholder = false;

    private TMP_Dropdown _dropdown;
    private bool _placeholderActive;
    public bool IsLanguageChosen => !_placeholderActive || _dropdown.value >= 0;
    public string SelectedLanguage =>
        _placeholderActive && _dropdown.value == 0 ? null : _dropdown.options[_dropdown.value].text;

    private void Awake()
    {
        _dropdown = GetComponent<TMP_Dropdown>();
        _dropdown.onValueChanged.AddListener(OnLanguageDropdownValueChanged);
        LocalizationManager.OnLanguageChanged += RefreshDropdownSelection;
        Initialize();
    }

    private void OnDestroy()
    {
        LocalizationManager.OnLanguageChanged -= RefreshDropdownSelection;
    }

    private void Initialize()
    {
        RefreshDropdownOptions();
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

        _dropdown.AddOptions(languages.Select(l => new TMP_Dropdown.OptionData(l)).ToList());
        _dropdown.SetValueWithoutNotify(_placeholderActive ? 0 : GetCurrentLanguageIndex(languages));
    }

    private int GetCurrentLanguageIndex(System.Collections.Generic.List<string> languages)
    {
        string current = LocalizationManager.CurrentLanguage;
        int index = languages.IndexOf(current);
        return index < 0 ? 0 : index;
    }

    private void RefreshDropdownSelection()
    {
        var languages = LocalizationManager.GetSupportedLanguages();
        if (languages.Count == 0) return;

        if (_placeholderActive)
        {
            _dropdown.SetValueWithoutNotify(0);
            return;
        }

        int index = GetCurrentLanguageIndex(languages);
        _dropdown.SetValueWithoutNotify(index);
    }

    private void OnLanguageDropdownValueChanged(int value)
    {
        if (_placeholderActive && value == 0) return;

        if (value > 0 && value < _dropdown.options.Count)
        {
            string lang = _dropdown.options[value].text;
            LocalizationManager.CurrentLanguage = lang;

            if (_placeholderActive)
            {
                _placeholderActive = false;
                RemovePlaceholder();
            }
        }
    }

    private void RemovePlaceholder()
    {
        if (_dropdown.options.Count > 0 && _dropdown.options[0].text == "--Language--")
        {
            _dropdown.options.RemoveAt(0);
            _dropdown.RefreshShownValue();

            int index = _dropdown.options.FindIndex(o => o.text == LocalizationManager.CurrentLanguage);
            _dropdown.SetValueWithoutNotify(index >= 0 ? index : 0);
        }
    }
}
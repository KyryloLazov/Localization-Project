#if UNITY_EDITOR
using System.Collections.Generic;
using Sirenix.OdinInspector;
#endif
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[DisallowMultipleComponent]
public class LocalizedText : MonoBehaviour
{
#if UNITY_EDITOR
    [ValueDropdown("GetAllLocalizationKeys", NumberOfItemsBeforeEnablingSearch = 15)]
#endif
    [SerializeField]
    private string _localizationKey;

    private Text _uiText;
    private TMP_Text _tmpText;

    private void OnEnable()
    {
        TryGetComponent(out _uiText);
        TryGetComponent(out _tmpText);

        LocalizationManager.OnLanguageChanged += UpdateText;
        UpdateText();
    }

    private void OnDisable()
    {
        LocalizationManager.OnLanguageChanged -= UpdateText;
    }

    private void OnValidate()
    {
        if (string.IsNullOrEmpty(_localizationKey)) return;
        UpdateText();
    }

    public void SetKey(string newKey)
    {
        _localizationKey = newKey;
        UpdateText();
    }

    private void UpdateText()
    {
        if (!enabled || string.IsNullOrWhiteSpace(_localizationKey))
        {
            return;
        }

        string value = LocalizationManager.Get(_localizationKey);

        if (_uiText != null)
        {
            _uiText.text = value;
        }

        if (_tmpText != null)
        {
            _tmpText.text = value;
        }
    }

#if UNITY_EDITOR
    private static IEnumerable<string> GetAllLocalizationKeys()
    {
        return LocalizationManager.GetAllLocalizationKeys();
    }
#endif
}

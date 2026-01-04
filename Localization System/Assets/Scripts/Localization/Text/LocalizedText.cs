using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class LocalizedTextUI : MonoBehaviour
{
    [SerializeField]
    private string _localizationKey;
    [SerializeField]
    private bool _applyFont = true;
    [SerializeField]
    private FontType _fontType = FontType.Text;

    private Text _uiText;
    private TMP_Text _tmpText;
    private LocalizationFontService _fontService;

    [Inject]
    public void Construct(LocalizationFontService fontService)
    {
        _fontService = fontService;
    }
    
    private void Awake()
    {
        TryGetComponent(out _uiText);
        TryGetComponent(out _tmpText);
    }

    private void Start()
    {
        if (Application.isPlaying)
        {
            UpdateText();
            UpdateFont();
        }
    }

    private void OnEnable()
    {
        LocalizationManager.OnLanguageChanged += HandleLanguageChange;
        LocalizationManager.OnInitialized += HandleLanguageChange;

        if (_applyFont && _fontService != null)
            _fontService.OnFontsChanged += UpdateFont;
        
        if (LocalizationManager.IsInitialized)
        {
            HandleLanguageChange();
        }
    }

    private void OnDisable()
    {
        LocalizationManager.OnLanguageChanged -= HandleLanguageChange;
        LocalizationManager.OnInitialized -= HandleLanguageChange;

        if (_applyFont && _fontService != null)
            _fontService.OnFontsChanged -= UpdateFont;
    }
    
    private void HandleLanguageChange()
    {
        UpdateText();
        UpdateFont();
    }

    public void SetKey(string newKey)
    {
        _localizationKey = newKey;
        UpdateText();
    }

    private void UpdateText()
    {
        if (!enabled) return;
        
        string value = string.IsNullOrWhiteSpace(_localizationKey)
            ? ""
            : LocalizationManager.Get(_localizationKey);
            
        if (_uiText != null) _uiText.text = value;
        if (_tmpText != null) _tmpText.text = value;
    }

    private void UpdateFont()
    {
        if (!enabled || !_applyFont || _fontService == null || _tmpText == null) return;
        
        TMP_FontAsset font = _fontService.GetCurrentFont(_fontType);
        
        if (font != null && _tmpText.font != font)
        {
            _tmpText.font = font;
        }
    }

#if UNITY_EDITOR
    private static IEnumerable<string> GetAllLocalizationKeys()
    {
        return LocalizationManager.GetAllLocalizationKeys();
    }
    private void OnValidate()
    {
        if (Application.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode) return;
        TryGetComponent(out _tmpText);
        TryGetComponent(out _uiText);
        if (LocalizationManager.IsInitialized)
            UpdateText();
    }
#endif
}
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class DistanceMeterTest : MonoBehaviour
{
    [SerializeField] private string _localizationKey;
    [SerializeField] private int _amount;
    
    private TextMeshProUGUI _text;

    private void OnEnable()
    {
        LocalizationManager.OnLanguageChanged += SetText;
    }

    private void OnDisable()
    {
        LocalizationManager.OnLanguageChanged -= SetText;
    }

    void Start()
    {
        _text = GetComponent<TextMeshProUGUI>();
    }

    private void SetText()
    {
        _text.text = LocalizationManager.Get(_localizationKey, _amount > 0 ? _amount : null);
    }
}

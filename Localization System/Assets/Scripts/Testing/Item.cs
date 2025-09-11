using System;
using TMPro;
using UnityEngine;

public class Item : MonoBehaviour
{
    [SerializeField] private string _itemName;
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

        //SetText();
    }

    private void SetText()
    {
        _text.text = LocalizationManager.Get(_itemName, _amount > 0 ? _amount : null);
    }
}

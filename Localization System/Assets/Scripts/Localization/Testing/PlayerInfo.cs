using TMPro;
using UnityEngine;

public class PlayerInfo : MonoBehaviour
{
    [SerializeField] private string _key = "player.info";
    
    [SerializeField] private string _playerName;
    [SerializeField] private int _age;
    
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
        if (string.IsNullOrWhiteSpace(_key))
        {
            _text.text = "";
            return;
        }
        
        _text.text = LocalizationManager.Get(_key, _playerName == "" ? null : _playerName, _age > 0 ? _age : null);
    }
}

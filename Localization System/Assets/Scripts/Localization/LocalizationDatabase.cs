using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using Sirenix.OdinInspector;
#endif

[CreateAssetMenu(fileName = "LocalizationDatabase", menuName = "Configs/Localization/Database")]
public class LocalizationDatabase : ScriptableObject
{
    [Serializable]
    public class LanguageData
    {
        public string LanguageCode;
        public List<Entry> Entries = new List<Entry>();
    }
    [Serializable]
    public class Entry
    {
        public string Key;
        public string Value;
    }
    
    [SerializeField]
    private List<LanguageData> _languages = new List<LanguageData>();
    
    public Dictionary<string, Dictionary<string, string>> ToDictionary()
    {
        var db = new Dictionary<string, Dictionary<string, string>>(System.StringComparer.OrdinalIgnoreCase);
        foreach (var langData in _languages)
        {
            var langDict = langData.Entries.ToDictionary(e => e.Key, e => e.Value);
            db[langData.LanguageCode.ToLowerInvariant()] = langDict;
        }
        return db;
    }

#if UNITY_EDITOR
    public void UpdateFrom(Dictionary<string, Dictionary<string, string>> newData)
    {
        _languages.Clear();
        foreach (var langPair in newData)
        {
            var langData = new LanguageData { LanguageCode = langPair.Key };
            foreach (var entryPair in langPair.Value)
            {
                langData.Entries.Add(new Entry { Key = entryPair.Key, Value = entryPair.Value });
            }
            _languages.Add(langData);
        }
        UnityEditor.EditorUtility.SetDirty(this);
    }
#endif
}
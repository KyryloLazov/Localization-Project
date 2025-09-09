#if UNITY_EDITOR
using Sirenix.OdinInspector;
#endif
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


    [CreateAssetMenu(fileName = "LocalizationDatabase", menuName = "Localization/Database")]
    public class LocalizationDatabase : ScriptableObject
    {
#if UNITY_EDITOR
        [TableMatrix(HorizontalTitle = "Languages", VerticalTitle = "Keys", IsReadOnly = true)]
#endif
        [SerializeField]
        private List<LocalizationEntry> _entries = new();

        private Dictionary<string, Dictionary<string, string>> _runtimeData;

        private void OnEnable()
        {
            BuildRuntimeData();
        }

        public string GetText(string key, string language)
        {
            if (_runtimeData == null)
            {
                BuildRuntimeData();
            }

            if (_runtimeData.TryGetValue(key, out var languageMap))
            {
                if (languageMap.TryGetValue(language, out var text))
                {
                    return text;
                }
            }
            
            return $"[{key}]";
        }
        
        public List<string> GetSupportedLanguages()
        {
            if (_runtimeData == null || _runtimeData.Count == 0)
            {
                return new List<string>();
            }

            return _runtimeData.Values
                .SelectMany(dict => dict.Keys)
                .Distinct()
                .ToList();
        }
        
        public IEnumerable<string> GetAllKeys()
        {
            if (_runtimeData == null)
            {
                BuildRuntimeData();
            }
            return _runtimeData.Keys;
        }

#if UNITY_EDITOR
        public void UpdateData(Dictionary<string, Dictionary<string, string>> newData)
        {
            _entries.Clear();
            var allLanguages = newData.Values
                .SelectMany(dict => dict.Keys)
                .Distinct()
                .ToList();

            foreach (var keyPair in newData)
            {
                var entry = new LocalizationEntry
                {
                    Key = keyPair.Key,
                    Translations = new List<Translation>()
                };

                foreach (var lang in allLanguages)
                {
                    entry.Translations.Add(new Translation
                    {
                        Language = lang,
                        Text = keyPair.Value.TryGetValue(lang, out var text) ? text : ""
                    });
                }
                _entries.Add(entry);
            }
            
            UnityEditor.EditorUtility.SetDirty(this);
            BuildRuntimeData();
        }
#endif
        private void BuildRuntimeData()
        {
            _runtimeData = new Dictionary<string, Dictionary<string, string>>();

            foreach (var entry in _entries)
            {
                if (string.IsNullOrWhiteSpace(entry.Key) || _runtimeData.ContainsKey(entry.Key))
                {
                    continue;
                }

                var languageMap = new Dictionary<string, string>();
                foreach (var translation in entry.Translations)
                {
                    if (!string.IsNullOrWhiteSpace(translation.Language) && !string.IsNullOrWhiteSpace(translation.Text))
                    {
                        languageMap[translation.Language.ToLowerInvariant()] = translation.Text.Replace("\\n", "\n");
                    }
                }
                _runtimeData[entry.Key] = languageMap;
            }
        }
    }

    [System.Serializable]
    public class LocalizationEntry
    {
#if UNITY_EDITOR
        [ReadOnly]
#endif
        public string Key;
        
#if UNITY_EDITOR
        [TableList]
#endif
        public List<Translation> Translations = new();
    }

    [System.Serializable]
    public class Translation
    {
#if UNITY_EDITOR
        [ReadOnly] [TableColumnWidth(100, Resizable = false)]
#endif
        public string Language;

        [TextArea] public string Text;
    }

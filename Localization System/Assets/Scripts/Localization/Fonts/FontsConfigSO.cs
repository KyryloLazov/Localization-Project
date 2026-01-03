using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Configs/Live/FontsConfigSO", fileName = "FontsConfigSO")]
public sealed class FontsConfigSO : LiveConfigSO
{
    private readonly Dictionary<string,string> _fontSpecs =
        new Dictionary<string,string>(System.StringComparer.OrdinalIgnoreCase);

    public void UpdateFromRemote(Dictionary<string,object> flatJson)
    {
        _fontSpecs.Clear();
        string prefix = Key + ".";
        foreach (var kvp in flatJson)
        {
            if (!kvp.Key.StartsWith(prefix, System.StringComparison.OrdinalIgnoreCase)) continue;
            string key = kvp.Key.Substring(prefix.Length);
            if (kvp.Value is string v)
                _fontSpecs[key.ToLowerInvariant()] = v;
        }
    }

    public string GetSpec(string lang, FontType type)
    {
        string langKey = lang.ToLowerInvariant();
        string finalKey = $"{langKey}_{type.ToString().ToLowerInvariant()}fonts";
        if (_fontSpecs.TryGetValue(finalKey, out var val))
        {
            return val;
        }
        return null;
    }
}

public enum FontType
{
    Text,
    Header
}
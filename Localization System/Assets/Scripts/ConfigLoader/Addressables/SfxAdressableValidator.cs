#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceLocations;

public static class SfxAddressablesValidator
{
    [MenuItem("Tools/Audio/Validate SFX Addressables")]
    public static void Validate()
    {
        var report = new System.Text.StringBuilder();
        var keys = new List<string>();
        // Собираем все ключи из локаторов
        foreach (var loc in Addressables.ResourceLocators)
        {
            foreach (var k in loc.Keys)
            {
                if (k is string s && s.StartsWith("sfx.", System.StringComparison.OrdinalIgnoreCase))
                    keys.Add(s);
            }
        }
        keys = keys.Distinct().OrderBy(s => s).ToList();

        var byAsset = new Dictionary<string, List<string>>(); // InternalId -> keys[]
        int ok = 0;

        foreach (var key in keys)
        {
            if (!Addressables.ResourceLocators.Any(l => l.Locate(key, typeof(AudioClip), out var _)))
            {
                report.AppendLine($"MISS: key='{key}' — не найдено в каталоге");
                continue;
            }

            // Берём первую локацию и её InternalId (путь/GUID)
            IResourceLocation loc = null;
            foreach (var l in Addressables.ResourceLocators)
                if (l.Locate(key, typeof(AudioClip), out var list) && list.Count > 0) { loc = list[0]; break; }

            var internalId = loc?.InternalId ?? "(null)";
            report.AppendLine($"OK:   {key} -> {internalId}");
            ok++;

            if (!byAsset.TryGetValue(internalId, out var list2))
                byAsset[internalId] = list2 = new List<string>();
            list2.Add(key);
        }

        report.AppendLine();
        report.AppendLine("=== Duplicates (один ассет на несколько ключей) ===");
        foreach (var kv in byAsset.Where(p => p.Value.Count > 1))
            report.AppendLine($"{kv.Key} <= {string.Join(", ", kv.Value)}");

        Debug.Log($"[SFX VALIDATOR] keys={keys.Count} ok={ok}\n" + report);
    }
}
#endif

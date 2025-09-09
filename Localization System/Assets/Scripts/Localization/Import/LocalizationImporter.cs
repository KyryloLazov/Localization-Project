#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;


public class LocalizationImporter
{
    private readonly string _csvUrl;

    public LocalizationImporter(string csvUrl)
    {
        _csvUrl = csvUrl;
    }

    public async UniTask<Dictionary<string, Dictionary<string, string>>> ImportAsync()
    {
        var lines = await LoadCsvLines(_csvUrl);
        return ParseLocalizationData(lines);
    }

    private async UniTask<List<string>> LoadCsvLines(string url)
    {
        using var client = new HttpClient();
        try
        {
            string content = await client.GetStringAsync(url);
            var lines = content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .ToList();

            if (lines.Count == 0)
            {
                Debug.LogWarning($"[Localization Importer] CSV file is empty at URL: {url}");
            }

            return lines;
        }
        catch (Exception e)
        {
            Debug.LogError($"[Localization Importer] Failed to load CSV from {url}. Error: {e.Message}");
            return new List<string>();
        }
    }

    private Dictionary<string, Dictionary<string, string>> ParseLocalizationData(List<string> lines)
    {
        var result = new Dictionary<string, Dictionary<string, string>>();
        if (lines.Count < 2)
        {
            return result;
        }

        var headers = SafeSplitCsvLine(lines[0]);
        if (headers.Count < 2)
        {
            Debug.LogError("[Localization Importer] CSV must have at least a 'Key' column and one language column.");
            return result;
        }

        int keyColumnIndex = headers.FindIndex(h => h.Trim().Equals("Key", StringComparison.OrdinalIgnoreCase));
        if (keyColumnIndex == -1)
        {
            Debug.LogError("[Localization Importer] CSV must contain a 'Key' column.");
            return result;
        }

        var languageColumns = new Dictionary<string, int>();
        for (int i = 0; i < headers.Count; i++)
        {
            if (i == keyColumnIndex) continue;

            string langCode = headers[i].Trim().ToLowerInvariant();
            if (!string.IsNullOrWhiteSpace(langCode))
            {
                languageColumns[langCode] = i;
            }
        }

        for (int i = 1; i < lines.Count; i++)
        {
            var cells = SafeSplitCsvLine(lines[i]);
            if (cells.Count <= keyColumnIndex) continue;

            string key = cells[keyColumnIndex].Trim();
            if (string.IsNullOrWhiteSpace(key)) continue;

            var translations = new Dictionary<string, string>();
            foreach (var langPair in languageColumns)
            {
                if (langPair.Value < cells.Count)
                {
                    string text = cells[langPair.Value].Trim();
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        translations[langPair.Key] = text;
                    }
                }
            }

            if (translations.Count > 0)
            {
                result[key] = translations;
            }
        }

        return result;
    }

    private static List<string> SafeSplitCsvLine(string line)
    {
        var result = new List<string>();
        bool inQuotes = false;
        string current = "";

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (c == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    current += '"';
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                result.Add(current);
                current = "";
            }
            else
            {
                current += c;
            }
        }

        result.Add(current);
        return result.Select(s => s.Trim()).ToList();
    }
}

#endif
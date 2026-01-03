using System.Collections.Generic;

public static class Flat
{
    public static string GetString(Dictionary<string, object> flat, string key, string fallback)
    {
        if (flat != null && flat.TryGetValue(key, out object v) && v != null)
        {
            string s = v.ToString();
            if (!string.IsNullOrEmpty(s)) return s;
        }
        return fallback;
    }
}
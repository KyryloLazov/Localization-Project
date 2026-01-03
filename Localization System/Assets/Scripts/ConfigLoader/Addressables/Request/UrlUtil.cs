using System;
using UnityEngine.Networking;

public static class UrlUtil
{
    public static string WithQuery(string url, string key, string value)
    {
        string sep = !string.IsNullOrEmpty(url) && url.Contains("?") ? "&" : "?";
        return $"{url}{sep}{UnityWebRequest.EscapeURL(key)}={UnityWebRequest.EscapeURL(value)}";
    }

    public static string WithCacheBuster(string url)
    {
        return WithQuery(url, "cb", Guid.NewGuid().ToString("N"));
    }

    public static string GetOrigin(string url)
    {
        if (string.IsNullOrEmpty(url)) return string.Empty;
        try
        {
            var u = new Uri(url);
            return u.GetLeftPart(UriPartial.Authority);
        }
        catch
        {
            return string.Empty;
        }
    }

    public static string Join(string a, string b)
    {
        if (string.IsNullOrEmpty(a)) return b ?? string.Empty;
        if (string.IsNullOrEmpty(b)) return a ?? string.Empty;
        string left = a.TrimEnd('/');
        string right = b.TrimStart('/');
        return left + "/" + right;
    }

    public static string AppendPath(string url, params string[] segments)
    {
        string result = url ?? string.Empty;
        if (segments == null) return result;
        for (int i = 0; i < segments.Length; i++)
        {
            result = Join(result, segments[i] ?? string.Empty);
        }
        return result;
    }

    public static string MakeAbsolute(string origin, string pathOrUrl)
    {
        if (string.IsNullOrEmpty(pathOrUrl)) return origin ?? string.Empty;
        if (pathOrUrl.Contains("://")) return pathOrUrl;
        return Join(origin, pathOrUrl);
    }
}
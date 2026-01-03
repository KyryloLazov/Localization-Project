using System.Collections.Generic;
using System.Globalization;

public sealed class RemoteConfigContext
{
    private readonly IReadOnlyDictionary<string, string> _map;

    public RemoteConfigContext(IReadOnlyDictionary<string, string> map)
    {
        _map = map;
    }

    public bool TryGetRaw(string key, out string value)
    {
        return _map.TryGetValue(key, out value);
    }

    public bool TryReadInt(string key, ref int target)
    {
        string raw;
        if (!_map.TryGetValue(key, out raw))
        {
            return false;
        }

        int value;
        if (!int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
        {
            return false;
        }

        target = value;
        return true;
    }

    public bool TryReadFloat(string key, ref float target)
    {
        string raw;
        if (!_map.TryGetValue(key, out raw))
        {
            return false;
        }

        float value;
        if (!float.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
        {
            return false;
        }

        target = value;
        return true;
    }

    public bool TryReadBool(string key, ref bool target)
    {
        string raw;
        if (!_map.TryGetValue(key, out raw))
        {
            return false;
        }

        if (raw == "1")
        {
            target = true;
            return true;
        }

        if (raw == "0")
        {
            target = false;
            return true;
        }

        bool value;
        if (!bool.TryParse(raw, out value))
        {
            return false;
        }

        target = value;
        return true;
    }

    public bool TryReadString(string key, ref string target)
    {
        string raw;
        if (!_map.TryGetValue(key, out raw))
        {
            return false;
        }

        target = raw;
        return true;
    }
}
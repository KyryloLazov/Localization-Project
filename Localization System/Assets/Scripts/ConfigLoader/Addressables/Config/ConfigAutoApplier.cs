using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

public static class ConfigAutoApplier
{
    public static void Apply(Dictionary<string, object> flat, IEnumerable<LiveConfigSO> targets, string ignoreKey)
    {
        if (flat == null || targets == null)
        {
            return;
        }

        foreach (LiveConfigSO so in targets)
        {
            if (so == null)
            {
                continue;
            }

            List<string> prefixes = new List<string>();
            if (!string.IsNullOrEmpty(so.Key))
            {
                prefixes.Add(so.Key);
            }

            string section = so.GetSection();
            if (!string.IsNullOrEmpty(section) && section != so.Key)
            {
                prefixes.Add(section);
            }

            Dictionary<string, object> slice = Slice(flat, prefixes, ignoreKey);
            if (slice.Count == 0)
            {
                continue;
            }

            ApplyToObject(so, slice);
            InvokeRaiseChanged(so);
        }
    }

    private static void InvokeRaiseChanged(LiveConfigSO so)
    {
        MethodInfo m = so.GetType().GetMethod(
            "RaiseChanged",
            BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

        if (m != null)
        {
            m.Invoke(so, null);
        }
    }

    private static Dictionary<string, object> Slice(Dictionary<string, object> flat, List<string> prefixes, string ignoreKey)
    {
        Dictionary<string, object> r = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < prefixes.Count; i++)
        {
            string p = prefixes[i];
            if (string.IsNullOrEmpty(p))
            {
                continue;
            }

            string pDot = p + ".";
            foreach (KeyValuePair<string, object> kv in flat)
            {
                if (!kv.Key.StartsWith(pDot, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                string name = kv.Key.Substring(pDot.Length);
                if (!string.IsNullOrEmpty(ignoreKey))
                {
                    if (string.Equals(name, ignoreKey, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    if (name.StartsWith(ignoreKey + ".", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                }

                r[name] = kv.Value;
            }
        }
        return r;
    }

    public static void ApplyToObject(object target, Dictionary<string, object> values)
    {
        if (target == null)
        {
            return;
        }

        Type t = target.GetType();
        BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        Dictionary<string, PropertyInfo> props = t
            .GetProperties(flags)
            .Where(p => p.CanRead && (p.SetMethod != null || p.GetMethod != null))
            .ToDictionary(p => p.Name, p => p, StringComparer.OrdinalIgnoreCase);

        Dictionary<string, FieldInfo> fields = t
            .GetFields(flags)
            .ToDictionary(f => f.Name, f => f, StringComparer.OrdinalIgnoreCase);

        foreach (KeyValuePair<string, object> kv in values)
        {
            string name = kv.Key;

            PropertyInfo p;
            if (props.TryGetValue(name, out p))
            {
                if (TrySetProperty(target, p, kv.Value))
                {
                    continue;
                }

                string backing = "<" + p.Name + ">k__BackingField";
                FieldInfo bf;
                if (fields.TryGetValue(backing, out bf))
                {
                    object v;
                    if (TryConvert(bf.FieldType, kv.Value, out v))
                    {
                        bf.SetValue(target, v);
                        continue;
                    }
                }
            }

            FieldInfo f;
            if (fields.TryGetValue(name, out f))
            {
                object v;
                if (TryConvert(f.FieldType, kv.Value, out v))
                {
                    f.SetValue(target, v);
                    continue;
                }
            }
        }
    }

    private static bool TrySetProperty(object target, PropertyInfo p, object raw)
    {
        MethodInfo set = p.SetMethod ?? p.DeclaringType
            .GetProperty(p.Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy)
            ?.SetMethod;

        if (set == null)
        {
            return false;
        }

        object v;
        if (!TryConvert(p.PropertyType, raw, out v))
        {
            return false;
        }

        set.Invoke(target, new object[] { v });
        return true;
    }

    private static bool TryConvert(Type to, object raw, out object result)
    {
        if (raw == null)
        {
            result = null;
            return !to.IsValueType || Nullable.GetUnderlyingType(to) != null;
        }

        Type u = Nullable.GetUnderlyingType(to) ?? to;
        if (u.IsAssignableFrom(raw.GetType()))
        {
            result = raw;
            return true;
        }

        try
        {
            if (u == typeof(string))
            {
                result = Convert.ToString(raw, CultureInfo.InvariantCulture);
                return true;
            }

            if (u == typeof(int))
            {
                result = Convert.ToInt32(raw, CultureInfo.InvariantCulture);
                return true;
            }

            if (u == typeof(float))
            {
                result = Convert.ToSingle(raw, CultureInfo.InvariantCulture);
                return true;
            }

            if (u == typeof(double))
            {
                result = Convert.ToDouble(raw, CultureInfo.InvariantCulture);
                return true;
            }

            if (u == typeof(bool))
            {
                string s = raw as string;
                if (s != null)
                {
                    if (string.Equals(s, "1", StringComparison.OrdinalIgnoreCase))
                    {
                        result = true;
                        return true;
                    }

                    if (string.Equals(s, "0", StringComparison.OrdinalIgnoreCase))
                    {
                        result = false;
                        return true;
                    }
                }

                result = Convert.ToBoolean(raw, CultureInfo.InvariantCulture);
                return true;
            }

            if (u.IsEnum)
            {
                string es = raw as string;
                if (es != null)
                {
                    result = Enum.Parse(u, es, true);
                    return true;
                }

                result = Enum.ToObject(u, Convert.ToInt32(raw, CultureInfo.InvariantCulture));
                return true;
            }
        }
        catch
        {
        }

        result = null;
        return false;
    }
}

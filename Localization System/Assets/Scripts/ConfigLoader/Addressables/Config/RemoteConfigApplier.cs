using System;
using System.Reflection;

public static class RemoteConfigApplier
{
    public static void ApplyToObject(object target, RemoteConfigContext context, string keyPrefix)
    {
        if (target == null)
        {
            return;
        }

        Type type = target.GetType();
        BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        FieldInfo[] fields = type.GetFields(flags);
        for (int i = 0; i < fields.Length; i++)
        {
            FieldInfo field = fields[i];
            if (field.IsStatic)
            {
                continue;
            }

            Type ft = field.FieldType;
            string key = keyPrefix + "." + field.Name;

            if (ft == typeof(int))
            {
                int value = (int)field.GetValue(target);
                if (context.TryReadInt(key, ref value))
                {
                    field.SetValue(target, value);
                }
            }
            else if (ft == typeof(float))
            {
                float value = (float)field.GetValue(target);
                if (context.TryReadFloat(key, ref value))
                {
                    field.SetValue(target, value);
                }
            }
            else if (ft == typeof(bool))
            {
                bool value = (bool)field.GetValue(target);
                if (context.TryReadBool(key, ref value))
                {
                    field.SetValue(target, value);
                }
            }
            else if (ft == typeof(string))
            {
                string value = (string)field.GetValue(target);
                if (context.TryReadString(key, ref value))
                {
                    field.SetValue(target, value);
                }
            }
        }

        PropertyInfo[] props = type.GetProperties(flags);
        for (int i = 0; i < props.Length; i++)
        {
            PropertyInfo prop = props[i];
            if (!prop.CanRead || !prop.CanWrite)
            {
                continue;
            }

            if (prop.GetIndexParameters().Length > 0)
            {
                continue;
            }

            Type pt = prop.PropertyType;
            string key = keyPrefix + "." + prop.Name;

            if (pt == typeof(int))
            {
                int value = (int)prop.GetValue(target, null);
                if (context.TryReadInt(key, ref value))
                {
                    prop.SetValue(target, value, null);
                }
            }
            else if (pt == typeof(float))
            {
                float value = (float)prop.GetValue(target, null);
                if (context.TryReadFloat(key, ref value))
                {
                    prop.SetValue(target, value, null);
                }
            }
            else if (pt == typeof(bool))
            {
                bool value = (bool)prop.GetValue(target, null);
                if (context.TryReadBool(key, ref value))
                {
                    prop.SetValue(target, value, null);
                }
            }
            else if (pt == typeof(string))
            {
                string value = (string)prop.GetValue(target, null);
                if (context.TryReadString(key, ref value))
                {
                    prop.SetValue(target, value, null);
                }
            }
        }
    }
}

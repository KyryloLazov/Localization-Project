using System;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class ConfigSectionAttribute : Attribute
{
    public readonly string Name;
    public ConfigSectionAttribute(string name) { Name = name; }
}
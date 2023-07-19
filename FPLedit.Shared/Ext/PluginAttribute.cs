using System;

namespace FPLedit.Shared;

/// <summary>
/// This attribute specifies sttaic properties of extensions and must be used on any <see cref="IPlugin"/> inheritors.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class PluginAttribute : Attribute
{
    /// <summary>
    /// Display name of the extension.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Minimum version of FPLedit this extension is compatible with.
    /// </summary>
    public string MinVer { get; }

    /// <summary>
    /// Maximum version of FPLedit this extension is compatible with.
    /// </summary>
    public string MaxVer { get; }

    /// <summary>
    /// Display name of the extension's author.
    /// </summary>
    public string? Author { get; set; }

    /// <summary>
    /// Web link to the extension's homepage.
    /// </summary>
    public string? Web { get; set; }

    /// <summary>
    /// Version of the extension itself.
    /// </summary>
    public string? Version { get; set; }

    public PluginAttribute(string name, string minVer, string maxVer)
    {
        Name = name;
        MinVer = minVer;
        MaxVer = maxVer;
    }
}
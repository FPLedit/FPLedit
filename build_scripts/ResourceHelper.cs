#nullable enable
using System;
using System.IO;
using System.Reflection;

internal static class ResourceHelper
{
    public static Stream GetResource(string dotFilePath)
    {
        var assembly = Assembly.GetCallingAssembly();
        return assembly.GetManifestResourceStream($"FPLedit.{dotFilePath}")
            ?? throw new Exception($"Requested resource FPLedit.{dotFilePath} not found!");
    }

    public static string GetStringResource(string dotFilePath)
    {
        using var stream = GetResource(dotFilePath);
        using var sr = new StreamReader(stream);
        return sr.ReadToEnd();
    }
}

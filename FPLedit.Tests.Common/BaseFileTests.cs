using System.IO;
using System.Reflection;
using System.Text;

namespace FPLedit.Tests.Common;

public abstract class BaseFileTests
{
    protected string Load(string dotPath)
    {
        var assembly = Assembly.GetCallingAssembly();
            
        return GetStringResource(assembly, assembly.GetName().Name + ".TestFiles." + dotPath);
    }

    protected Stream PrepareTemp(string text)
    {
        var ms = new MemoryStream();
        using (var sw = new StreamWriter(ms, new UTF8Encoding(false), 1024, true))
            sw.Write(text);
        ms.Seek(0, SeekOrigin.Begin);
        return ms;
    }
        
    private static Stream GetResource(Assembly assembly, string dotFilePath)
    {
        return assembly.GetManifestResourceStream(dotFilePath)!;
    }

    private static string GetStringResource(Assembly assembly, string dotFilePath)
    {
        using var stream = GetResource(assembly, dotFilePath);
        using var sr = new StreamReader(stream);
        return sr.ReadToEnd();
    }
}
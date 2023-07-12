using FPLedit.Shared;
using System;
using System.IO;
using FPLedit.DebugDump.Forms;

namespace FPLedit.DebugDump;

[Plugin("DebugDump", Vi.PFrom, Vi.PUpTo, Author = "Manuel Huber")]
public sealed class Plugin : IPlugin, IDisposable
{
    private DebugListener? listener;

    public void Init(IPluginInterface pluginInterface, IComponentRegistry componentRegistry)
    {
        if (pluginInterface.Settings.Get<bool>("dump.record"))
        {
            var defaultPath = pluginInterface.GetTemp("..");
            var basePath = pluginInterface.Settings.Get("dump.path", defaultPath);
            if (basePath == "")
                basePath = defaultPath;
            basePath = Path.GetFullPath(basePath);
            if (!Directory.Exists(basePath))
                Directory.CreateDirectory(basePath);
                
            listener = new DebugListener(basePath);
            listener.StartSession(pluginInterface);
        }
            
        componentRegistry.Register<ISettingsControl>(new SettingsFormHandler());
    }

    public void Dispose() => listener?.Dispose();
}
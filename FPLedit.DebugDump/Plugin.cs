using Eto.Forms;
using FPLedit.Shared;
using FPLedit.Shared.Filetypes;
using FPLedit.Shared.UI;
using System;
using System.IO;
using System.Linq;
using System.Xml;
using FPLedit.DebugDump.Forms;

namespace FPLedit.DebugDump
{
    [Plugin("DebugDump", Vi.PFrom, Vi.PUpTo, Author = "Manuel Huber")]
    public sealed class Plugin : IPlugin, IDisposable
    {
        private IPluginInterface pluginInterface;
        private DebugListener listener;

        public void Init(IPluginInterface pluginInterface, IComponentRegistry componentRegistry)
        {
            this.pluginInterface = pluginInterface;

            if (pluginInterface.Settings.Get<bool>("dump.record"))
            {
                var defaultPath = pluginInterface.GetTemp("..");
                var basePath = pluginInterface.Settings.Get("dump.path", defaultPath);
                if (basePath == "")
                    basePath = defaultPath;
                basePath = Path.GetFullPath(basePath);
                if (!Directory.Exists(basePath))
                    Directory.CreateDirectory(basePath);
                
                listener = new DebugListener();
                listener.StartSession(pluginInterface, basePath);
            }

            pluginInterface.ExtensionsLoaded += PluginInterfaceOnExtensionsLoaded;
        }

        private void PluginInterfaceOnExtensionsLoaded(object sender, EventArgs e)
        {
            var menu = pluginInterface.HelpMenu as ButtonMenuItem;
#pragma warning disable CA2000
            menu!.CreateItem("Debug Dum&p", true, (s, args) =>
            {
                using (var sf = new SettingsForm(pluginInterface.Settings))
                    sf.ShowModal();
            });
#pragma warning disable CA2000
        }

        public void Dispose() => listener?.Dispose();
    }
}
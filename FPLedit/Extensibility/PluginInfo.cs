using FPLedit.Shared;
using System;
using System.Reflection;

namespace FPLedit.Extensibility
{
    internal sealed class PluginInfo
    {
        private readonly IPlugin plugin;
        
        public bool IsBuiltin { get; }

        public bool Enabled { get; set; }

        public string Name { get; private set; }

        public string Author { get; private set; }

        public string Version { get; private set; }

        public string Url { get; private set; }

        public string FullName { get; }

        public SecurityContext SecurityContext { get; }

        public PluginInfo(IPlugin plugin, SecurityContext securityContext, bool isBuiltin = false)
        {
            FullName = plugin.GetType().FullName;
            ExtractPluginInformation(plugin.GetType());
            this.plugin = plugin;
            this.IsBuiltin = isBuiltin;
            Enabled = true;
            SecurityContext = securityContext;
        }

        public PluginInfo(Type type, SecurityContext securityContext)
        {
            FullName = type.FullName;
            ExtractPluginInformation(type);
            Enabled = false;
            SecurityContext = securityContext;
        }

        private void ExtractPluginInformation(Type t)
        {
            var a = t.GetCustomAttribute<PluginAttribute>(false);
            Name = a?.Name ?? "<Fehler beim Laden>";
            if (a == null)
                return;
            Author = a.Author;
            Version = a.Version;
            Url = a.Web;
        }

        public void TryInit(IPluginInterface pluginInterface, IComponentRegistry componentRegistry)
        {
            try
            {
                plugin.Init(pluginInterface, componentRegistry);
            }
            catch (Exception ex)
            {
                pluginInterface.Logger.Error("Fehler beim Initialisieren einer Erweiterung: " + plugin.GetType().FullName + ": " + ex.Message);
            }
        }
    }
}

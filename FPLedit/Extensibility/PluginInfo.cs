#nullable enable
using FPLedit.Shared;
using System;
using System.Reflection;

namespace FPLedit.Extensibility
{
    internal sealed class PluginInfo
    {
        internal readonly IPlugin? Plugin;
        
        public bool IsBuiltin { get; }

        public bool Enabled { get; set; }

        public string Name { get; private set; } = "<Fehler beim Laden>";

        public string? Author { get; private set; }

        public string? Version { get; private set; }

        public string? Url { get; private set; }

        public string FullName { get; }

        public SecurityContext SecurityContext { get; }

        public PluginInfo(IPlugin plugin, SecurityContext securityContext, bool isBuiltin = false)
        {
            FullName = plugin.GetType().FullName!;
            ExtractPluginInformation(plugin.GetType());
            Plugin = plugin;
            IsBuiltin = isBuiltin;
            Enabled = true;
            SecurityContext = securityContext;
        }

        public PluginInfo(Type type, SecurityContext securityContext)
        {
            FullName = type.FullName!;
            ExtractPluginInformation(type);
            Enabled = false;
            SecurityContext = securityContext;
        }

        private void ExtractPluginInformation(Type t)
        {
            var a = t.GetCustomAttribute<PluginAttribute>(false);
            Name = a?.Name ?? T._("<Fehler beim Laden>");
            if (a == null)
                return;
            Author = a.Author;
            Version = a.Version;
            Url = a.Web;
        }

        public void TryInit(IPluginInterface pluginInterface, IComponentRegistry componentRegistry)
        {
            if (Plugin == null) return;
            try
            {
                Plugin.Init(pluginInterface, componentRegistry);
            }
            catch (Exception ex)
            {
                pluginInterface.Logger.Error(T._("Fehler beim Initialisieren einer Erweiterung: {0}: + {1}", Plugin.GetType().FullName!, ex.Message));
            }
        }
    }
}

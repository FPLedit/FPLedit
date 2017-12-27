using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FPLedit
{
    internal class PluginInfo
    {
        private IPlugin plugin;

        public bool Enabled { get; set; }

        public string Name { get; private set; }

        public string Author { get; private set; }

        public string Version { get; set; }

        public string Url { get; set; }

        public string FullName { get; private set; }

        public PluginInfo(IPlugin plugin)
        {
            FullName = plugin.GetType().FullName;
            ExtractPluginInformation(plugin.GetType());
            this.plugin = plugin;
            Enabled = true;
        }

        public PluginInfo(Type type)
        {
            FullName = type.FullName;
            ExtractPluginInformation(type);
            Enabled = false;
        }

        private void ExtractPluginInformation(Type t)
        {
            var attrs = t.GetCustomAttributes(typeof(PluginAttribute), false);
            Name = "<Fehler beim Laden>";
            if (attrs.Length != 1)
                return;
            var a = attrs[0] as PluginAttribute;
            Author = a.Author;
            Version = a.Version;
            Url = a.Web;
            Name = a.Name;
        }

        public void TryInit(IInfo info)
        {
            try
            {
                plugin.Init(info);
            }
            catch (Exception ex)
            {
                info.Logger.Error("Fehler beim Initialisieren einer Erweiterung: " + plugin.GetType().FullName + ": " + ex.Message);
            }
        }
    }
}

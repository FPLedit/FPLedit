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
            Name = GetDisplayName(plugin.GetType());
            this.plugin = plugin;
            Enabled = true;
        }

        public PluginInfo(Type type)
        {
            FullName = type.FullName;
            Name = GetDisplayName(type);
            Enabled = false;
        }

        private string GetDisplayName(Type t)
        {
            var attrs = t.GetCustomAttributes(typeof(PluginAttribute), false);
            if (attrs.Length != 1)
                return "<Fehler beim Laden>";
            var a = attrs[0] as PluginAttribute;
            Author = a.Author;
            Version = a.Version;
            Url = a.Web;
            return a.Name;
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

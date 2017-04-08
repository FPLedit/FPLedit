using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FPLedit
{
    public class PluginContainer : IPlugin
    {
        public IPlugin Plugin { get; private set; }

        public PluginContainer(IPlugin plugin)
        {
            Plugin = plugin;
        }

        public string Name
        {
            get
            {
                try {
                    return Plugin.Name;
                }
                catch {
                    return "<" + Plugin.GetType().FullName + ">";
                }
            }
        }

        public void Init(IInfo info)
        {
            try {
                Plugin.Init(info);
            }
            catch {
                info.Logger.Error("Fehler beim Initialisieren einer Erweiterung: " + Plugin.GetType().FullName);
            }
        }
    }
}

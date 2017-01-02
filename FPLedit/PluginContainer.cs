using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FPLedit
{
    class PluginContainer : IPlugin
    {
        IPlugin plg;

        public PluginContainer(IPlugin plugin)
        {
            plg = plugin;
        }

        public string Name
        {
            get
            {
                try {
                    return plg.Name;
                }
                catch {
                    return "<Fehler beim Laden>";
                }
            }
        }

        public void Init(IInfo info)
        {
            try {
                plg.Init(info);
            }
            catch {
                //TODO: Log
            }
        }
    }
}

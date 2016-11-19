using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace FPLedit
{
    /// <summary>
    /// Stammt vom Projekt OctoAwesome, http://octoawesome.net, wurde aber abgeändert
    /// </summary>
    public class ExtensionManager
    {
        public List<IPlugin> EnabledPlugins { get; private set; }

        public List<IPlugin> DisabledPlugins { get; private set; }

        public ExtensionManager()
        {
            List<Assembly> assemblies = new List<Assembly>();
            DirectoryInfo dir = new DirectoryInfo(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));

            foreach (var file in dir.GetFiles("*.dll"))
            {
                try
                {
                    var assembly = Assembly.LoadFile(file.FullName);
                    assemblies.Add(assembly);
                }
                catch
                {
                }
            }

            List<IPlugin> result = new List<IPlugin>();
            foreach (var assembly in assemblies)
            {
                try
                {
                    foreach (var type in assembly.GetTypes())
                    {
                        if (!type.IsClass) continue;
                        if (!type.IsPublic) continue;
                        if (type.IsAbstract) continue;
                        if (type == typeof(IPlugin)) continue;                        

                        if (typeof(IPlugin).IsAssignableFrom(type))
                        {
                            IPlugin plugin = (IPlugin)Activator.CreateInstance(type);

                            bool enabled = true; //TODO: lookup config

                            if (enabled)
                                EnabledPlugins.Add(plugin);
                            else
                                DisabledPlugins.Add(plugin);
                        }
                    }
                }
                catch
                {
                }
            }
            EnabledPlugins = result;
        }
    }
}

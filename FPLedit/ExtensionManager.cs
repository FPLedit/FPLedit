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

            EnabledPlugins = new List<IPlugin>();
            DisabledPlugins = new List<IPlugin>();

            string[] enabledExtensions = SettingsManager.Get("EnabledExtensions", "").Split(';');
            bool enableAll = enabledExtensions.Length < 0;

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

                            bool enabled = enableAll || enabledExtensions.Contains(type.FullName); //TODO: lookup config

                            if (enabled)
                                EnabledPlugins.Add(new PluginContainer(plugin));
                            else
                                DisabledPlugins.Add(new PluginContainer(plugin));
                        }
                    }
                }
                catch
                {
                }
            }
        }
    }
}

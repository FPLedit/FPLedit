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
        public List<PluginContainer> EnabledPlugins { get; private set; }

        public List<PluginContainer> DisabledPlugins { get; private set; }

        public ExtensionManager(ILog log)
        {
            List<Assembly> assemblies = new List<Assembly>();
            DirectoryInfo dir = new DirectoryInfo(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location));

            foreach (var file in dir.GetFiles("*.dll"))
            {
                try
                {
                    var assembly = Assembly.LoadFrom(file.FullName);
                    assemblies.Add(assembly);
                }
                catch (FileLoadException ex)
                {
                    log.Warning("Erweiterung " + file.Name + " konnte nicht geladen werden, bitte überprüfen ob diese noch geblockt ist!");
                }
                catch
                {
                }
            }

            EnabledPlugins = new List<PluginContainer>();
            DisabledPlugins = new List<PluginContainer>();

            string[] enabledExtensions = SettingsManager.Get("extmgr.enabled", "").Split(';');
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

                            bool enabled = enableAll || enabledExtensions.Contains(type.FullName);

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

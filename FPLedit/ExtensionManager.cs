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
            var path = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            DirectoryInfo dir = new DirectoryInfo(path);

            foreach (var file in dir.GetFiles("*.dll"))
            {
                try
                {
                    var assembly = Assembly.LoadFrom(file.FullName);
                    assemblies.Add(assembly);
                }
                catch (FileLoadException)
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

            foreach (var assembly in assemblies)
            {
                try
                {
                    foreach (var type in assembly.GetTypes())
                    {
                        if (!type.IsClass || !type.IsPublic || type.IsAbstract || type == typeof(IPlugin))
                            continue;

                        if (!typeof(IPlugin).IsAssignableFrom(type))
                            continue;

                        IPlugin plugin = (IPlugin)Activator.CreateInstance(type);

                        bool enabled = enabledExtensions.Contains(type.FullName);

                        if (enabled)
                            EnabledPlugins.Add(new PluginContainer(plugin));
                        else
                            DisabledPlugins.Add(new PluginContainer(plugin));
                    }
                }
                catch
                {
                }
            }
        }
    }
}

using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace FPLedit
{
    internal class ExtensionManager
    {
        public List<PluginInfo> Plugins { get; private set; }

        public bool EnabledModified { get; private set; }

        private ISettings settings;

        public ExtensionManager(ILog log, ISettings settings)
        {
            Plugins = new List<PluginInfo>();
            this.settings = settings;
            var enabledPlugins = settings.Get("extmgr.enabled", "").Split(';');

            var path = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            DirectoryInfo dir = new DirectoryInfo(path);

            foreach (var file in dir.GetFiles("*.dll"))
            {
                try
                {
                    var assembly = Assembly.LoadFrom(file.FullName);

                    foreach (var type in assembly.GetTypes())
                    {
                        if (!type.IsClass || !type.IsPublic || type.IsAbstract || type == typeof(IPlugin))
                            continue;

                        if (!typeof(IPlugin).IsAssignableFrom(type))
                            continue;

                        if (type.GetCustomAttributes(typeof(PluginAttribute), false).Length != 1)
                            continue;

                        bool enabled = enabledPlugins.Contains(type.FullName);

                        if (enabled)
                        {
                            IPlugin plugin = (IPlugin)Activator.CreateInstance(type);

                            var info = new PluginInfo(plugin);
                            Plugins.Add(info);
                        }
                        else
                        {
                            var info = new PluginInfo(type);
                            Plugins.Add(info);
                        }
                    }
                }
                catch (FileLoadException)
                {
                    log.Warning("Erweiterung " + file.Name + " konnte nicht geladen werden, bitte überprüfen ob diese noch geblockt ist!");
                }
                catch
                {
                }
            }
        }

        public void Activate(PluginInfo info)
        {
            if (info.Enabled)
                return;
            info.Enabled = true;
            EnabledModified = true;
        }

        public void Deactivate(PluginInfo info)
        {
            if (!info.Enabled)
                return;
            info.Enabled = false;
            EnabledModified = true;
        }

        public void WriteConfig()
        {
            settings.Set("extmgr.enabled", string.Join(";", Plugins.Where(p => p.Enabled).Select(p => p.FullName)));
        }
    }
}

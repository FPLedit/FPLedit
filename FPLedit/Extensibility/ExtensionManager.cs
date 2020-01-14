using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace FPLedit.Extensibility
{
    internal class ExtensionManager
    {
        public List<PluginInfo> Plugins { get; private set; }

        public bool EnabledModified { get; private set; }

        private readonly IPluginInterface pluginInterface;
        private readonly UpdateManager update;

        public ExtensionManager(IPluginInterface pluginInterface, UpdateManager update)
        {
            this.pluginInterface = pluginInterface;
            this.update = update;
        }

        public void LoadExtensions()
        {
            Plugins = new List<PluginInfo>();

            var signatureVerifier = new AssemblySignatureVerifier();

            var enabledPlugins = pluginInterface.Settings.Get("extmgr.enabled", "").Split(';');
            var currentVersion = update.CurrentVersion;

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

                        object[] attribs = type.GetCustomAttributes(typeof(PluginAttribute), false);
                        if (attribs.Length != 1)
                            continue;

                        var attr = (PluginAttribute)attribs[0];
                        if (Version.TryParse(attr.MinVer, out Version min) && VersionCompare(currentVersion, min) < 0)
                            continue; // Inkompatible Erweiterung (Programm zu alt)
                        if (Version.TryParse(attr.MaxVer, out Version max) && VersionCompare(currentVersion, max) > 0)
                            continue; // Inkompatible Erweiterung (Programm zu neu)

                        var securityContext = signatureVerifier.Validate(file.FullName);

                        if (enabledPlugins.Contains(type.FullName))
                        {
                            IPlugin plugin = (IPlugin)Activator.CreateInstance(type);

                            var pluginInfo = new PluginInfo(plugin, securityContext);
                            Plugins.Add(pluginInfo);
                        }
                        else
                        {
                            var pluginInfo = new PluginInfo(type, securityContext);
                            Plugins.Add(pluginInfo);
                        }
                    }
                }
                catch (FileLoadException)
                {
                    pluginInterface.Logger.Warning("Erweiterung " + file.Name + " konnte nicht geladen werden, bitte überprüfen ob diese noch geblockt ist!");
                }
                catch
                {
                }
            }
        }

        public void Activate(PluginInfo pluginInfo)
        {
            if (pluginInfo.Enabled)
                return;
            pluginInfo.Enabled = true;
            EnabledModified = true;
        }

        public void Deactivate(PluginInfo pluginInfo)
        {
            if (!pluginInfo.Enabled)
                return;
            pluginInfo.Enabled = false;
            EnabledModified = true;
        }

        public void WriteConfig()
        {
            pluginInterface.Settings.Set("extmgr.enabled", string.Join(";", Plugins.Where(p => p.Enabled).Select(p => p.FullName)));
        }

        private int VersionCompare(Version v1, Version v2)
        {
            if (v1 == null || v2 == null)
                return 1;

            bool skipBuild = v2.Build == -1,
                skipRevision = v2.Revision == -1;

            if (v1.Major != v2.Major)
                return v1.Major > v2.Major ? 1 : -1;

            if (v1.Minor != v2.Minor)
                return v1.Minor > v2.Minor ? 1 : -1;

            if (v1.Build != v2.Build && !skipBuild)
                return v1.Build > v2.Build ? 1 : -1;

            if (v1.Revision != v2.Revision && !skipRevision)
                return v1.Revision > v2.Revision ? 1 : -1;

            return 0;
        }

        public void InitActivatedExtensions()
        {
            new Editor.EditorPlugin().Init(pluginInterface);

            var enabled_plgs = Plugins.Where(p => p.Enabled);
            foreach (var plugin in enabled_plgs)
                plugin.TryInit(pluginInterface);
        }
    }
}

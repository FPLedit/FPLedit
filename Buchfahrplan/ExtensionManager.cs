using Buchfahrplan.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Buchfahrplan
{
    /// <summary>
    /// Stammt vom Projekt OctoAwesome, http://octoawesome.net, wurde aber abgeändert
    /// </summary>
    public static class ExtensionManager
    {
        private static List<IPlugin> plugins;
        public static List<IPlugin> Plugins
        {
            get
            {
                if (plugins != null)
                    return plugins;

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
                                result.Add((IPlugin)Activator.CreateInstance(type));
                        }
                    }
                    catch
                    {
                    }
                }
                plugins = result;
                return result;
            }
        }
    }
}

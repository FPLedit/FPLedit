using FPLedit.Shared;
using System;
using System.IO;
using System.Reflection;
using System.Security.Permissions;

namespace FPLedit.Templating
{
    public class TemplateSandbox : MarshalByRefObject
    {
        public void InstallResolver(string path)
        {
            AppDomain.CurrentDomain.AssemblyResolve += (s, e) =>
            {
                var name = new AssemblyName(e.Name);
                var fn = name.Name + ".dll";

                var p = Path.Combine(path, fn);
                if (File.Exists(p))
                    return Assembly.LoadFrom(p);
                return null;
            };
        }

        public string RunInAppDomain(Timetable tt, string assemblyPath)
        {
            var assembly = Assembly.LoadFrom(assemblyPath);

            return InvokeTemplate(assembly, tt);
        }

        private string InvokeTemplate(Assembly assembly, Timetable tt)
        {
            foreach (Type type in assembly.GetTypes())
            {
                var mi = type.GetMethod("Render", BindingFlags.Public | BindingFlags.Static);
                if (mi != null && mi.GetParameters().Length == 1)
                    return (string)mi.Invoke(null, new object[] { tt });
            }
            return null;
        }
    }
}

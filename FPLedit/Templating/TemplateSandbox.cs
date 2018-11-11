using FPLedit.Shared;
using System;
using System.IO;
using System.Reflection;

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

        public string CompileInAppDomain(Timetable tt, string code, string[] refs)
        {
            Compiler engine = new Compiler();
            var ret = engine.RunTemplate(code, refs, tt);
            return ret;
        }
    }
}

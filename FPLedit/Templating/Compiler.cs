using FPLedit.Shared;
using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace FPLedit.Templating
{
    internal class Compiler
    {
        public string RunTemplate(string code, string[] references, Timetable tt)
        {
            Assembly assembly = GetAssembly(code, references);
            return InvokeTemplate(assembly, tt);
        }

        private Assembly GetAssembly(string code, string[] references)
        {
            var wd = Environment.CurrentDirectory;
            Environment.CurrentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            var provider = new CSharpCodeProvider();

            var cparams = new CompilerParameters()
            {
                CompilerOptions = "/target:library",
                GenerateExecutable = false,
                GenerateInMemory = true,
            };

            cparams.ReferencedAssemblies.Add("mscorlib.dll");
            cparams.ReferencedAssemblies.Add("System.Core.dll");
            cparams.ReferencedAssemblies.Add("FPLedit.Shared.dll");
            cparams.ReferencedAssemblies.AddRange(references);

            var results = provider.CompileAssemblyFromSource(cparams, code);

            if (results.Errors.Count > 0)
            {
                foreach (CompilerError error in results.Errors)
                    throw new Exception("Kompilierungsfehler: " + error.ErrorText);

                return null;
            }

            Environment.CurrentDirectory = wd;

            return results.CompiledAssembly;
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

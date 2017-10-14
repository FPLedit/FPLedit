using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace TemplaetingTest.Templating
{
    internal class Compiler
    {
        public string RunTemplate(string code, string[] references)
        {
            Assembly assembly = GetAssembly(code, references);
            return InvokeTemplate(assembly);
        }

        private Assembly GetAssembly(string code, string[] references)
        {
            var provider = new CSharpCodeProvider();

            var cparams = new CompilerParameters()
            {
                CompilerOptions = "/target:library",
                GenerateExecutable = false,
                GenerateInMemory = true,
            };

            cparams.ReferencedAssemblies.Add("mscorlib.dll");
            cparams.ReferencedAssemblies.AddRange(references);

            var results = provider.CompileAssemblyFromSource(cparams, code);

            if (results.Errors.Count > 0)
            {
                foreach (CompilerError error in results.Errors)
                    throw new Exception("Kompilierungsfehler: " + error.ErrorText);

                return null;
            }

            return results.CompiledAssembly;
        }

        private string InvokeTemplate(Assembly assembly)
        {
            foreach (Type type in assembly.GetTypes())
            {
                var mi = type.GetMethod("Render", BindingFlags.Public | BindingFlags.Static);
                if (mi != null && mi.GetParameters().Length == 0)
                    return (string)mi.Invoke(null, null);
            }
            return null;
        }
    }
}

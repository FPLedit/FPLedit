﻿using FPLedit.Shared;
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
        public static string CompilerDebugTemp => Path.Combine(Path.GetTempPath(), "fpledit-compiler");

        public string CompileAssembly(string code, string[] references)
        {
            var wd = Environment.CurrentDirectory;
            Environment.CurrentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            var provider = new CSharpCodeProvider();

            var guid = Guid.NewGuid().ToString();

            var cparams = new CompilerParameters()
            {
                CompilerOptions = "/target:library",
                GenerateExecutable = false,
                OutputAssembly = Path.Combine(CompilerDebugTemp, guid + ".dll"),
                GenerateInMemory = false,
                TempFiles = new TempFileCollection(CompilerDebugTemp, false),
            };

#if TMPL_DEBUG
            cparams.IncludeDebugInformation = true;
            cparams.TempFiles.KeepFiles = true;
#endif

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

            return results.PathToAssembly;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;

namespace FPLedit.Config
{
    internal static class OptionsParser
    {
        private static readonly string[] known_flags = {
            "--mp-log",
            "--tmpl-debug",
        };

        private static readonly List<string> PresentFlags = new List<string>();

        public static string OpenFilename { get; private set; }

        public static bool MPCompatLog => PresentFlags.Contains("--mp-log");

        public static bool TemplateDebug => PresentFlags.Contains("--tmpl-debug");
        
        public static bool CrashReporterDebug => PresentFlags.Contains("--crash-debug");

        public static void Init(string[] args)
        {
            foreach (var arg in args)
            {
                if (known_flags.Contains(arg))
                    PresentFlags.Add(arg);
                else if (OpenFilename == null)
                    OpenFilename = arg;
                else
                    throw new Exception("Unbekanntes Flag oder doppelter Dateiname angegeben!");
            }
        }
    }
}

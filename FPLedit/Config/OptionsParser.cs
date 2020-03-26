using System;
using System.Collections.Generic;
using System.Linq;

namespace FPLedit.Config
{
    internal static class OptionsParser
    {
        private static readonly string[] knownFlags = {
            "--mp-log",
            "--tmpl-debug",
            "--log-console",
        };

        private static readonly List<string> presentFlags = new List<string>();

        public static string OpenFilename { get; private set; }

        public static bool ConsoleLog => presentFlags.Contains("--mp-log") || presentFlags.Contains("--log-console");

        public static bool TemplateDebug => presentFlags.Contains("--tmpl-debug");
        
        public static bool CrashReporterDebug => presentFlags.Contains("--crash-debug");

        public static void Init(string[] args)
        {
            foreach (var arg in args)
            {
                if (knownFlags.Contains(arg))
                    presentFlags.Add(arg);
                else if (OpenFilename == null)
                    OpenFilename = arg;
                else
                    throw new Exception("Unbekanntes Flag oder doppelter Dateiname angegeben!");
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPLedit.Config
{
    internal static class OptionsParser
    {
        private static readonly string[] known_flags = new[]
        {
            "--mp-log",
            "--tmpl-debug",
        };

        private static readonly List<string> present_flags = new List<string>();

        public static string OpenFilename { get; private set; }

        public static bool MPCompatLog => present_flags.Contains("--mp-log");

        public static bool TemplateDebug => present_flags.Contains("--tmpl-debug");

        public static void Init(string[] args)
        {
            foreach (var arg in args)
            {
                if (known_flags.Contains(arg))
                    present_flags.Add(arg);
                else if (OpenFilename == null)
                    OpenFilename = arg;
                else
                    throw new Exception("Unbekanntes Flag oder doppelter Dateiname angegeben!");
            }
        }
    }
}

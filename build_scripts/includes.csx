/*
 * FPLedit Release-Prozess
 * Includes zur Generierung einer einheitlichen Lizenz
 * Version 0.4 / (c) Manuel Huber 2020
 */

using System;
using System.IO;
using System.Diagnostics;

var template_header = @"FPLedit Version {0}

(c) 2015-{1} Manuel Huber
https://fahrplan.manuelhu.de/

";

string GetLicense(string version)
{
    var script_path = Path.GetDirectoryName(Environment.GetCommandLineArgs()[1]);
    var info = File.ReadAllText(Path.Combine(script_path, "Info.txt"));

    var tmpl = template_header + AddNewlines(info);
    var year = DateTime.Now.Year;
    return string.Format(tmpl, version, year);
}

string GetProductVersion(string output_path)
{
    var main_path = Path.Combine(output_path, "FPLedit.exe");
    var version = FileVersionInfo.GetVersionInfo(main_path).ProductVersion;
    Console.WriteLine("Verwendete Versionsinfo: " + version + "\n");
    return version;
}

string AddNewlines(string text, int length = 75)
{
    var ret = "";
    string[] sections = text.Split(new[] { "\n", "\r\n" }, StringSplitOptions.None);
    foreach (var sect in sections)
    {
        string[] parts = sect.Split(' ');
        var block = parts[0];
        int z = parts[0].Length;
        for (int i = 1; i < parts.Length; i++)
        {
            if ((z + 1 + parts[i].Length) > length)
            {
                block += Environment.NewLine + parts[i];
                z = parts[i].Length;
            }
            else
            {
                block += " " + parts[i];
                z += 1 + parts[i].Length;
            }
        }
        ret += block + Environment.NewLine;
    }

    return ret;
}

void Warning(string text) {
    Console.WriteLine($"BUILD_SCRIPTS(1,1,1,2): warning: [BUILD] {text}");
}

void Error(string text) {
    Console.WriteLine($"BUILD_SCRIPTS(1,1,1,2): error: [BUILD] {text}");
}

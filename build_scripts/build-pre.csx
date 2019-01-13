/*
 * FPLedit Build-Prozess
 * Kopiert die Eto-Plattform-Assemblies
 * Aufruf: build-pre.csx $(SolutionDir) $(TargetDir)
 * Version 0.1 / (c) Manuel Huber 2019
 */

using System;
using System.IO;

Console.WriteLine("Prebuild step: Copying eto platform binaries");

var code_path = Path.GetFullPath(Args[0]);
var bin_path = Path.GetFullPath(Args[1]);

// Auflistung, der Eto-Plattform-Dateien, die kopiert werden.
var files = new string[]
{
    "Eto.Gtk3.dll",
    "Eto.Wpf.dll",
};

var eto_base = Path.Combine(code_path, "libs");

foreach (var file in files)
{
    var src = Path.Combine(eto_base, file);
    var dest = Path.Combine(bin_path, file);
    Console.WriteLine($"{src} --> {dest}");
    File.Copy(src, dest, true);
}

Console.WriteLine("Finished prebuild!");

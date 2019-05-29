/*
 * FPLedit Build-Prozess
 * Kopiert die Eto-Plattform-Assemblies
 * Aufruf: build-pre.csx $(SolutionDir) $(TargetDir)
 * Version 0.2 / (c) Manuel Huber 2019
 */

using System;
using System.IO;

Console.WriteLine("Prebuild step: Copying eto platform binaries");

var code_path = Path.GetFullPath(Args[0]);
var bin_path = Path.GetFullPath(Args[1]);

var eto_base = Path.Combine(code_path, "libs");
var files = new DirectoryInfo(eto_base).GetFiles("*.dll");

foreach (var file in files)
{
    var dest = Path.Combine(bin_path, file.Name);
    Console.WriteLine($"{file.FullName} --> {dest}");
    file.CopyTo(dest, true);
}

Console.WriteLine("Finished prebuild!");

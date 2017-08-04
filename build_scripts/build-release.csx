/*
 * FPLedit Release-Prozess
 * Erstellt aus einem Ordner mit Kompilaten eine ZIP-Datei
 * Aufruf mit Pfad zum Ordner der Kompilate
 * Version 0.3 / (c) Manuel Huber 2017
 */

#r "System.IO.Compression.FileSystem.dll"
#load "license.csx"

using System;
using System.IO;
using System.Diagnostics;
using System.IO.Compression;

Console.WriteLine("Post-Build: Erstelle Binärpaket!");

var output_path = Path.GetFullPath(Args[0]);

var version = GetProductVersion(output_path);

/*
 * TASK: Cleanup release files
 */
DirectoryInfo info = new DirectoryInfo(output_path);

Console.WriteLine("Entferne PDBs");
var files = info.GetFiles("*.pdb");
foreach (var f in files)
{
    File.Delete(f.FullName);
    Console.WriteLine(f.Name);
}

Console.WriteLine("Entferne Installer!\n");
files = info.GetFiles("FPLedit.Installer.exe");
foreach (var f in files)
{
    File.Delete(f.FullName);
    Console.WriteLine(f.Name);
}

/*
 * TASK: Build new license file
 */
Console.WriteLine("Generiere neue README-Datei");
var license = GetLicense(version);
var license_path = Path.Combine(output_path, "README_LICENSE.txt");
File.WriteAllText(license_path, license);

/*
 * TASK: Build ZIP file
 */
Console.WriteLine("Erstelle ZIP-Datei");
var result_path = Path.Combine(output_path, "..", $"fpledit-{version}.zip");

if (File.Exists(result_path))
    Console.WriteLine(String.Format($"build-release.csx(1,1,1,2): warning: ZIP-Datei {result_path} existiert bereits und wurde nicht erneut generiert!"));
else
{
    ZipFile.CreateFromDirectory(output_path, result_path);
    Console.WriteLine("ZIP-Datei {0} erstellt!\n", result_path);
}

Console.WriteLine("Post-Build erfolgreich abgeschlossen!");

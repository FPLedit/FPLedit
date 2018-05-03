/*
 * FPLedit Release-Prozess
 * Erstellt aus einem Ordner mit Kompilaten eine ZIP-Datei
 * Aufruf mit Pfad zum Ordner der Kompilate
 * Version 0.5 / (c) Manuel Huber 2018
 */

#r "System.IO.Compression.FileSystem.dll"
#load "includes.csx"

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

Console.WriteLine("Entferne Installer!");
files = info.GetFiles("FPLedit.Installer.exe");
foreach (var f in files)
{
    File.Delete(f.FullName);
    Console.WriteLine(f.Name);
}

/*
 * TASK: Move eto files
 */
Console.WriteLine("Verschiebe Eto-Bibliotheken!");
files = info.GetFiles("Eto.*");
var eto_dest = info.CreateSubdirectory("eto");
foreach (var f in files)
{
    var fname = Path.Combine(eto_dest.FullName, f.Name);
    f.MoveTo(fname);
    Console.WriteLine(f.Name);
}

Console.WriteLine();

/*
 * TASK: Build new license file
 */
Console.WriteLine("Generiere neue README-Datei");
var license = GetLicense(version);
var license_path = Path.Combine(output_path, "README_LICENSE.txt");
File.WriteAllText(license_path, license);

/*
 * TASK: Add offline documentation file
 */
var doc = Environment.GetEnvironmentVariable("FPLEDIT_DOK");
var doc_generated = false;
if (doc != null && File.Exists(doc))
{
    Console.WriteLine("Kopiere Offline-Dokumentation");
    var doc_path = Path.Combine(output_path, "doku.html");
    File.Copy(doc, doc_path);
    doc_generated = true;
}
else
{
    Console.WriteLine(String.Format($"build-release.csx(1,1,1,2): warning: [BUILD] Umgebungsvariable FPLEDIT_DOK nicht gesetzt bzw. die Datei (= Wert der Variablen) existiert nicht! Das generierte Programmpaket enthält keine Dokumentation!"));
}

/*
 * TASK: Build ZIP file
 */
Console.WriteLine("Erstelle ZIP-Datei");
var result_path = Path.Combine(output_path, "..", $"fpledit-{version}{(doc_generated ? "" : "-nodoc")}.zip");

if (File.Exists(result_path))
    Console.WriteLine(String.Format($"build-release.csx(1,1,1,2): warning: [BUILD] ZIP-Datei {result_path} existiert bereits und wurde nicht erneut generiert!"));
else
{
    ZipFile.CreateFromDirectory(output_path, result_path);
    Console.WriteLine("ZIP-Datei {0} erstellt!\n", result_path);
}

Console.WriteLine("Post-Build erfolgreich abgeschlossen!");

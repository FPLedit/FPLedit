/*
 * FPLedit Release-Prozess
 * Erstellt aus dem Ordner mit dem Sourcecode eine ZIP-Datei
 * Aufruf mit Pfad zum Ordner des Sourcecodes
 * Version 0.5 / (c) Manuel Huber 2020
 */

#r "System.IO.Compression.FileSystem.dll"
#load "includes.csx"

using System;
using System.IO;
using System.Linq;
using System.Diagnostics;
using System.IO.Compression;

Console.WriteLine("Post-Build: Erstelle Sourcecodepaket!");

var code_path = Path.GetFullPath(Args[0]);
var bin_path = Path.GetFullPath(Args[1]);
var target_path = Path.Combine(Path.GetTempPath(), "fpledit_build");

var version = GetProductVersion(bin_path);

/*
 * TASK: Copy source files to %TEMP%
 */
Console.WriteLine("Kopiere Dateien nach " + target_path);
CopyFilesRecursively(new DirectoryInfo(code_path), new DirectoryInfo(target_path));

/*
 * TASK: Build new license file
 */
Console.WriteLine("Generiere neue README-Datei");
var license = GetLicense(version);
var license_path = Path.Combine(target_path, "README_LICENSE.txt");
File.WriteAllText(license_path, license);

/*
 * TASK: Build ZIP file
 */
Console.WriteLine("Erstelle ZIP-Datei");
var result_path = Path.Combine(bin_path, "..", $"fpledit-{version}-src.zip");

if (File.Exists(result_path))
    Warning($"ZIP-Datei {result_path} existiert bereits und wurde nicht erneut generiert!");
else
{
    ZipFile.CreateFromDirectory(target_path, result_path);
    Console.WriteLine("ZIP-Datei {0} erstellt!\n", result_path);
}

Directory.Delete(target_path, true);

Console.WriteLine("Post-Build erfolgreich abgeschlossen!");

public static void CopyFilesRecursively(DirectoryInfo source, DirectoryInfo target)
{
    var dir_exceptions = new[] { ".git", ".vs", "bin", "obj", ".idea" };
    var ext_exceptions = new[] { ".user", ".gitignore", ".gitattributes", "increment" };

    int copied = 0;
    foreach (DirectoryInfo dir in source.GetDirectories())
    {
        if (dir_exceptions.Contains(dir.Name))
            continue;
        CopyFilesRecursively(dir, target.CreateSubdirectory(dir.Name));
        copied++;
    }
    foreach (FileInfo file in source.GetFiles())
    {
        if (ext_exceptions.Any(e => file.Name.EndsWith(e)))
            continue;
        file.CopyTo(Path.Combine(target.FullName, file.Name));
        copied++;
    }
    if (copied == 0) // Leere Ordner nicht mitkopieren
        target.Delete();
}

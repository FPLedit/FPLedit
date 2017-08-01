/*
 * FPLedit Release-Prozess
 * Erstellt aus einem Ordner mit Kompilaten eine ZIP-Datei
 * Aufruf mit Pfad zum Ordner der Kompilate
 * Version 0.2 / (c) Manuel Huber 2017
 */

#r "System.IO.Compression.FileSystem.dll"

using System;
using System.IO;
using System.Diagnostics;
using System.IO.Compression;

Console.WriteLine("Starte Post-Build für Release!");

var output_path = Path.GetFullPath(Args[0]);
var main_path = Path.Combine(output_path, "FPLedit.exe");

var version = FileVersionInfo.GetVersionInfo(main_path).ProductVersion;
Console.WriteLine("Verwendete Versionsinfo: " + version + "\n");

/*
 * TASK: Cleanup release files
 */
DirectoryInfo info = new DirectoryInfo(output_path);

Console.WriteLine("Entferne PDBs");
var files = info.GetFiles("*.pdb");
foreach (var f in files) {
    File.Delete(f.FullName);
    Console.WriteLine(f.Name);
}

Console.WriteLine("Entferne Installer!\n");
files = info.GetFiles("FPLedit.Installer.exe");
foreach (var f in files) {
    File.Delete(f.FullName);
    Console.WriteLine(f.Name);
}

/*
 * TASK: Build new license file
 */
Console.WriteLine("Generiere neue README-Datei");
var year = DateTime.Now.Year;
var license = string.Format(GetLicenseTemplate(), version, year);
var license_path = Path.Combine(output_path, "README_LICENSE.txt");
File.WriteAllText(license_path, license);

/*
 * TASK: Build ZIP file
 */
Console.WriteLine("Erstelle ZIP-Datei");
var result_path = Path.Combine(output_path, "..", $"fpledit-{version}.zip");

if (File.Exists(result_path)) {
    Console.WriteLine(String.Format($"build-release.csx(1,1,1,2): warning: ZIP-Datei {result_path} existiert bereits!"));
    Console.WriteLine(String.Format($"build-release.csx(1,1,1,2): warning: ZIP-Datei wurde nicht generiert!"));
    return;
}
ZipFile.CreateFromDirectory(output_path, result_path);
Console.WriteLine("ZIP-Datei {0} erstellt!\n", result_path);

Console.WriteLine("Post-Build erfolgreich abgeschlossen!");


string GetLicenseTemplate()
 => @"FPLedit Version {0}

(c) 2015-{1} Manuel Huber
https://fahrplan.manuelhu.de/

FPledit darf für den nicht-kommerziellen Gebrauch (dies schließt die
Veröffentlichung damit erstellter Fahrpläne auf privaten Websites
ausdrücklich ein) kostenlos heruntergeladen und verwendet werden.
Die Weitergabe oder Bereitstellung des Programms über eine öffentliche
Plattform oder gegen Entgelt ist nur nach vorheriger Zustimmung des
Programmautors gestattet. Verweisen Sie bitte stattdessen auf die
offizielle Website des Programms.
Eine kommerzielle Nutzung des Programms bedarf meiner vorherigen Zustimmung.

FPledit ist ein Projekt, das primär auf (Modell-)Eisenbahnfreunde abzielt.
Die Fahrpläne sind nicht nach den Betriebsrichtlinien irgendeiner
Bahngesellschaft gestaltet und sind für den Betriebsdienst nicht geeignet!

Der Autor dieses Programms haftet nicht für Schäden an Soft- oder
Hardware oder Vermögensschäden, die durch das Benutzen des Programms entstehen,
es sei denn diese beruhen auf einem grob fahrlässigen oder vorsälichen
Handeln des Autors, seiner Erfüllungsgehilfen oder seiner gesetzlichen
Vertreter. Für Schäden an der Gesundheit, dem Körper oder dem Leben des
Nutzers haftet der Autor uneingeschränkt. Ebenso haftet er für die
Verletzung von Pflichten, die zur Erreichung des Vertragszwecks von besonderer
Bedeutung sind (Kardinalspflichten).";

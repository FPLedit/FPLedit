using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: AssemblyCompany("Manuel Huber")]
[assembly: AssemblyProduct("FPLedit")]
[assembly: AssemblyCopyright("Copyright © 2016-2018 Manuel Huber")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Versionsinformationen für eine Assembly bestehen aus den folgenden vier Werten:
//
//      Hauptversion
//      Nebenversion
//      Buildnummer
//      Revision
//
// Sie können alle Werte angeben oder die standardmäßigen Build- und Revisionsnummern
// übernehmen, indem Sie "*" eingeben:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("1.0.0.0")] // Do not change
[assembly: AssemblyFileVersion("2.0.0.0")] // File Version -> increment for fixes

[assembly: AssemblyInformationalVersion("2.0.0")] // Display Version

// Central version information used in plugins
internal static class Pvi
{
    public const string Version = "2.0.0"; // Version of plugin (normally equasls assembly major.minor
    public const string UpTo = "2.0"; // Compatible up to (normally "Version" without patch)
    public const string From = "2.0.0"; // Compatible from - normally equals assembly version
}
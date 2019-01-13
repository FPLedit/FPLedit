using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: AssemblyCompany("Manuel Huber")]
[assembly: AssemblyProduct("FPLedit")]
[assembly: AssemblyCopyright("Copyright © 2015-2019 Manuel Huber")]
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
[assembly: AssemblyFileVersion(Vi.FileVersion)] // File Version -> increment for fixes

[assembly: AssemblyInformationalVersion(Vi.InformationalVersion)] // Display Version

// Central version information used in plugins & main application
internal static class Vi
{
    /*
     * FOR EVERY RELEASE: Increment InformationalVersion an maybe also PUpTo.
     * The rest ist updated automatically
     */
    public const string InformationalVersion = "2.1.0";
    public const string PUpTo = "2.1"; // Compatible up to (normally "Version" without patch)

    /*
     * It shouldn't be necessary to update the following entries.
     */
    public const string FileVersion = InformationalVersion + ".0";

    public const string PVersion = InformationalVersion; // Version of plugin (normally equasls assembly major.minor.patch)
    public const string PFrom = InformationalVersion; // Compatible from - normally equals assembly version
}

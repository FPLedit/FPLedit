using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: AssemblyCompany("Manuel Huber")]
[assembly: AssemblyProduct("FPLedit")]
[assembly: AssemblyCopyright("Copyright © 2015-2020 Manuel Huber")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Version number format for assemblies: Major.Minor.Build.Revision
// [assembly: AssemblyVersion("1.0.*")] to auto-generate build- and  revision numbers
[assembly: AssemblyVersion("1.0.0.0")] // Do not change
[assembly: AssemblyFileVersion(Vi.FileVersion)] // File Version -> increment for fixes

[assembly: AssemblyInformationalVersion(Vi.InformationalVersion)] // Display Version

// Include assembly configuration hints
#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif

// Central version information used in plugins & main application
internal static class Vi
{
    /*
     * FOR EVERY RELEASE: Increment the following version entries
     * The rest ist updated automatically
     */
    private const string Major = "2", Minor = "2", Patch = "0";

    /*
     * It shouldn't be necessary to update the following entries.
     */
    public const string InformationalVersion = Major + "." + Minor + "." + Patch;
    public const string PUpTo = Major + "." + Minor; // Compatible up to (normally "Version" without patch)

    public const string FileVersion = InformationalVersion + ".0";

    public const string PVersion = InformationalVersion; // Version of plugin (normally equasls assembly major.minor.patch)
    public const string PFrom = InformationalVersion; // Compatible from - normally equals assembly version
}

internal class AssemblyVersionFlagAttribute : Attribute
{
    public string Flag { get; }

    public AssemblyVersionFlagAttribute(string flag)
    {
        Flag = flag;
    }
}

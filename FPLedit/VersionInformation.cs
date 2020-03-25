using System;
using System.Diagnostics;
using System.Reflection;

namespace FPLedit
{
    public static class VersionInformation
    {
        public static string BaseVersionString => FileVersionInfo.GetVersionInfo(PathManager.Instance.AppFilePath).ProductVersion;
        public static Version AppBaseVersion => new Version(BaseVersionString);
        public static string VersionFlag => typeof(MainForm).Assembly.GetCustomAttribute<AssemblyVersionFlagAttribute>()?.Flag;
        public static string DisplayVersion => BaseVersionString + (VersionFlag != null ? "-" + VersionFlag : "");

        public static string OsVersion => Environment.OSVersion.ToString();

        public static string RuntimeVersion => System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription;
    }
}
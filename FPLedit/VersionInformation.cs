using System;
using System.Diagnostics;
using System.Reflection;

namespace FPLedit
{
    public class VersionInformation
    {
        public string BaseVersionString => FileVersionInfo.GetVersionInfo(PathManager.Instance.AppFilePath).ProductVersion;
        public Version AppBaseVersion => new Version(BaseVersionString);
        public string VersionFlag => typeof(MainForm).Assembly.GetCustomAttribute<AssemblyVersionFlagAttribute>()?.Flag;
        public string DisplayVersion => BaseVersionString + (VersionFlag != null ? "-" + VersionFlag : "");

        public string OsVersion => Environment.OSVersion.ToString();

        public string RuntimeVersion => System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription;

        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once EmptyConstructor
        public VersionInformation()
        {
        }

        private static VersionInformation current;
        public static VersionInformation Current
        {
            get
            {
                if (current == null)
                    current = new VersionInformation();
                return current;
            }
        }
    }
}
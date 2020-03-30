using System;
using System.Diagnostics;
using System.Reflection;

namespace FPLedit
{
    public sealed class VersionInformation
    {
        private string BaseVersionString => FileVersionInfo.GetVersionInfo(PathManager.Instance.AppFilePath).FileVersion;
        public Version AppBaseVersion => new Version(BaseVersionString);
        public string DisplayVersion => FileVersionInfo.GetVersionInfo(PathManager.Instance.AppFilePath).ProductVersion;

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
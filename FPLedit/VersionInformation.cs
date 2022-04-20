using System;

namespace FPLedit
{
    public sealed class VersionInformation
    {
        private string BaseVersionString => Vi.PVersion;
        public Version AppBaseVersion => new Version(BaseVersionString);
        public string DisplayVersion => Vi.DisplayVersion;
        public string VersionSuffix => Vi.VersionSuffix;
        public bool IsDevelopmentVersion => Vi.IsNonFinal;

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
                current ??= new VersionInformation();
                return current;
            }
        }
    }
}
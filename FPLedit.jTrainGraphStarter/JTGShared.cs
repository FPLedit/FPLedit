using FPLedit.Shared;
using System.IO;
using System.Text.RegularExpressions;

namespace FPLedit.jTrainGraphStarter
{
    internal static class JtgShared
    {
        public const string DEFAULT_FILENAME = "jTrainGraph_311.jar";
        public const TimetableVersion DEFAULT_TT_VERSION = TimetableVersion.JTG3_1;

        public static JtgCompatibility JtgCompatCheck(string jTgPath)
        {
            var fn = Path.GetFileNameWithoutExtension(jTgPath);

            var match = Regex.Match(fn, @"jTrainGraph_(\d)(\d{2})");
            if (match != null && match.Success && match.Groups.Count == 3)
            {
                var major = int.Parse(match.Groups[1].Value);
                var minor = int.Parse(match.Groups[2].Value);

                if (major == 2)
                    return new JtgCompatibility(minor > 1, TimetableVersion.JTG2_x); // Ab 2.01
                if (major == 3 && minor < 10)
                    return new JtgCompatibility(minor > 2, TimetableVersion.JTG3_0); // Ab 3.03
                if (major == 3)
                    return new JtgCompatibility(minor >= 10 && minor < 20, TimetableVersion.JTG3_1);
                return new JtgCompatibility(false); // Neue Hauptversion, wahrscheinlich inkompatibel
            }
            return new JtgCompatibility(true); // Hier gibt es keine Informationen, also "kompatibel"
        }
    }

    internal struct JtgCompatibility
    {
        public readonly bool Compatible;
        public readonly TimetableVersion? Version;

        public JtgCompatibility(bool compatible, TimetableVersion? version = null)
        {
            Compatible = compatible;
            Version = compatible ? version : null;
        }
    }
}

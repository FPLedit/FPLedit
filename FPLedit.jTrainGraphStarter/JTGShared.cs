using FPLedit.Shared;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace FPLedit.jTrainGraphStarter
{
    internal static class JtgShared
    {
        public const string DEFAULT_FILENAME = "jTrainGraph_320.jar";
        public const TimetableVersion DEFAULT_TT_VERSION = TimetableVersion.JTG3_2;
        
        public static JtgCompatibility JtgCompatCheck(string jTgPath)
        {
            var versions = TimetableVersionExt.GetAllVersionInfos()
                .Where(c => c.Compatibility == TtVersionCompatType.ReadWrite)
                .Where(c => c.JtgVersionCompatibility.Any())
                .SelectMany(c => 
                    c.JtgVersionCompatibility.Select(j => (version: c.Version, pattern: j.version)))
                .ToArray();

            var fn = Path.GetFileNameWithoutExtension(jTgPath);

            var match = Regex.Match(fn, @"jTrainGraph_(\d)(\d{2})");
            if (match.Success && match.Groups.Count == 3)
            {
                var major = int.Parse(match.Groups[1].Value);
                var minor = int.Parse(match.Groups[2].Value);

                foreach (var (version, pattern) in versions)
                {
                    var regex = new Regex(@$"^{pattern.Replace(".", "\\.").Replace("*", "\\d*")}$");
                    if (regex.IsMatch($"{major}.{minor}")) 
                        return new JtgCompatibility(true, version);
                }
                
                return new JtgCompatibility(false); // New major version, probably incompatible.
            }
            return new JtgCompatibility(true); // No information available, so it is "compatibile".
        }
    }

    internal readonly struct JtgCompatibility
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

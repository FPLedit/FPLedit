using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FPLedit.jTrainGraphStarter
{
    internal class JTGShared
    {
        public const string DEFAULT_FILENAME = "jTrainGraph_303.jar";
        public const int DEFAULT_TT_VERSION = 009;

        public static JTGCompatibility JTGCompatCheck(string jTGPath)
        {
            var fn = Path.GetFileNameWithoutExtension(jTGPath);

            var match = Regex.Match(fn, @"jTrainGraph_(\d)(\d{2})");
            if (match != null && match.Groups.Count == 3)
            {
                var major = int.Parse(match.Groups[1].Value);
                var minor = int.Parse(match.Groups[2].Value);

                if (major == 2)
                    return new JTGCompatibility(minor > 1, TimetableVersion.JTG2_x); // Ab 2.01
                else if (major == 3)
                    return new JTGCompatibility(minor > 2, TimetableVersion.JTG3_0); // Ab 3.03
                else
                    return new JTGCompatibility(false); // Neue Hauptversion, wahrscheinlich inkompatibel
            }
            return new JTGCompatibility(true); // Hier gibt es keine Informationen, also "kompatibel"
        }
    }

    internal struct JTGCompatibility
    {
        public bool Compatible;
        public TimetableVersion? Version;

        public JTGCompatibility(bool compatible, TimetableVersion? version = null)
        {
            Compatible = compatible;
            Version = compatible ? version : null;
        }
    }
}

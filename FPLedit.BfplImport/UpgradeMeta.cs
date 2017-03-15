using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace FPLedit.BfplImport
{
    internal static class UpgradeMeta
    {
        public static Dictionary<string, string> Upgrade(EntityType type, Dictionary<string, string> meta)
        {
            if (type == EntityType.Timetable)
                return UpgradeTimetable(meta);
            else if (type == EntityType.Train)
                return UpgradeTrain(meta);
            else
                return UpgradeStation(meta);
        }

        static Dictionary<string, string> UpgradeTrain(Dictionary<string, string> meta)
        {
            var upgraded = new Dictionary<string, string>();

            if (meta.TryGetValue("Color", out string o))
                upgraded["cl"] = UpgradeColor(o, "schwarz");
            if (meta.TryGetValue("Draw", out o))
                upgraded["sh"] = o.ToLower();
            if (meta.TryGetValue("Width", out o))
                upgraded["sz"] = o;

            return upgraded;
        }

        static Dictionary<string, string> UpgradeTimetable(Dictionary<string, string> meta)
        {
            var upgraded = new Dictionary<string, string>();

            if (meta.TryGetValue("StartTime", out string o))
                upgraded["tMin"] = UpgradeTime(o);
            if (meta.TryGetValue("EndTime", out o))
                upgraded["tMax"] = UpgradeTime(o);
            if (meta.TryGetValue("ShowDays", out o))
                upgraded["d"] = o;
            if (meta.TryGetValue("DisplayKilometre", out o))
                upgraded["shKm"] = o.ToLower();
            if (meta.TryGetValue("StationLines", out o))
                upgraded["shV"] = o.ToLower();
            if (meta.TryGetValue("BgColor", out o))
                upgraded["bgC"] = UpgradeColor(o, "weiß");

            return upgraded;
        }

        private static string UpgradeTime(string input)
        {
            var s = input.Split(':');
            var h = int.Parse(s[0]);
            var m = int.Parse(s[1]);
            return (h * 60 + m).ToString();
        }

        private static string UpgradeColor(string input, string def)
        {
            Color c = ColorTranslator.FromHtml(input);
            string hex = string.Format("#{0:X2}{1:X2}{2:X2}", c.R, c.G, c.B);
            var colors = new Dictionary<string, string>()
            {
                ["#000000"] = "schwarz",
                ["#808080"] = "grau",
                ["#FFFFFF"] = "weiß",
                ["#FF0000"] = "rot",
                ["#FFA500"] = "orange",
                ["#FFFF00"] = "gelb",
                ["#0000FF"] = "blau",
                ["#ADD8E6"] = "hellblau",
                ["#008000"] = "grün",
                ["#006400"] = "dunkelgrün",
                ["#A52A2A"] = "braun",
                ["#FF00FF"] = "magenta",
            };
            if (colors.ContainsKey(hex))
                return colors[hex];
            return def;
        }

        static Dictionary<string, string> UpgradeStation(Dictionary<string, string> meta)
        {
            var upgradeMap = new Dictionary<string, string>()
            {
                ["MaxVelocity"] = "fpl-vmax",
            };

            return meta.Where(kvp => upgradeMap.ContainsKey(kvp.Key)).ToDictionary(kvp => upgradeMap[kvp.Key], kvp => kvp.Value);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace FPLedit.Shared
{
    public static class ColorHelper
    {
        private static Dictionary<string, string> Colors = new Dictionary<string, string>()
        {
            ["Black"] = "#000000",
            ["Gray"] = "#808080",
            ["White"] = "#FFFFFF",
            ["Red"] = "#FF0000",
            ["Orange"] = "#FFA500",
            ["Yellow"] = "#FFFF00",
            ["Blue"] = "#0000FF",
            ["LightBlue"] = "#ADD8E6",
            ["Green"] = "#008000",
            ["DarkGreen"] = "#006400",
            ["Brown"] = "#A52A2A",
            ["Magenta"] = "#FF00FF",
        };

        public static string[] ColorNames
        {
            get { return Colors.Keys.ToArray(); }
        }


        public static string NameFromHex(string hex)
        {
            return Colors.FirstOrDefault(c => c.Value == hex).Key ?? hex;
        }

        public static string HexFromName(string name)
        {
            return Colors.FirstOrDefault(c => c.Key == name).Key ?? name;
        }

        public static Color ColorFromHex(string hex)
        {
            return ColorTranslator.FromHtml(hex);
        }

        public static Color ColorFromName(string name)
        {
            return ColorFromHex(HexFromName(name));
        }
    }
}

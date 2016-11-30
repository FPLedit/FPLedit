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
            ["Schwarz"] = "#000000",
            ["Grau"] = "#808080",
            ["Weiß"] = "#FFFFFF",
            ["Rot"] = "#FF0000",
            ["Orange"] = "#FFA500",
            ["Gelb"] = "#FFFF00",
            ["Blau"] = "#0000FF",
            ["Hellblau"] = "#ADD8E6",
            ["Grün"] = "#008000",
            ["Dunkelgrün"] = "#006400",
            ["Braun"] = "#A52A2A",
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
            return Colors.FirstOrDefault(c => c.Key == name).Value ?? name;
        }

        public static Color ColorFromHex(string hex)
        {
            return ColorTranslator.FromHtml(hex);
        }

        public static Color ColorFromName(string name)
        {
            return ColorFromHex(HexFromName(name));
        }

        public static string NameFromColor(Color c)
        {
            return NameFromHex(ColorTranslator.ToHtml(c));
        }
    }
}

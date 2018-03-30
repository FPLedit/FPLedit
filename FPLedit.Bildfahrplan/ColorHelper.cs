using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace FPLedit.BildfahrplanExport
{
    public static class ColorHelper
    {
        private static Dictionary<string, Color> Colors = new Dictionary<string, Color>()
        {
            ["Schwarz"] = Color.Black,
            ["Grau"] = Color.Gray,
            ["Weiß"] = Color.White,
            ["Rot"] = Color.Red,
            ["Orange"] = Color.Orange,
            ["Gelb"] = Color.Yellow,
            ["Blau"] = Color.Blue,
            ["Hellblau"] = Color.LightBlue,
            ["Grün"] = Color.Green,
            ["Dunkelgrün"] = Color.DarkGreen,
            ["Braun"] = Color.Brown,
            ["Magenta"] = Color.Magenta,
        };

        public static string[] ColorNames
            => Colors.Keys.ToArray();

        public static Color ColorFromHex(string hex)
            => ColorTranslator.FromHtml(hex);

        public static Color ColorFromName(string name)
            => Colors[name];

        public static string NameFromColor(Color color)
            => Colors.FirstOrDefault(c => c.Value == color).Key ?? HexFromColor(color);

        public static string HexFromColor(Color c)
            => string.Format("#{0:X2}{1:X2}{2:X2}", c.R, c.G, c.B);
    }
}

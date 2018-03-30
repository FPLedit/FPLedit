using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace FPLedit.BildfahrplanExport
{
    internal abstract class Style
    {
        private Timetable _parent;

        public Style(Timetable tt)
        {
            _parent = tt;
        }

        #region Fonts
        protected Font ParseFont(string def)
        {
            if (def == null || !def.StartsWith("font(") || !def.EndsWith(")"))
                return new Font("Arial", 9); // Keine valide Font-Definition

            var parts = def.Substring(5, def.Length - 6).Split(';');
            var family = GetFontFamily(parts[0]);
            var style = (FontStyle)int.Parse(parts[1]);
            var size = int.Parse(parts[2]);

            return new Font(family, size, style);
        }

        private FontFamily GetFontFamily(string name)
        {
            switch (name.ToLower())
            {
                case "sansserif":
                case "dialog":
                case "dialoginput":
                    return FontFamily.GenericSansSerif;
                case "monospaced":
                    return FontFamily.GenericMonospace;
                case "serif":
                    return FontFamily.GenericSerif;
            }
            return new FontFamily(name);
        }

        protected string FontToString(Font font)
        {
            var family = GetFontName(font.FontFamily);
            var style = (int)font.Style;
            return "font(" + family + ";" + style + ";" + (int)font.Size + ")";
        }

        private string GetFontName(FontFamily family)
        {
            if (family == FontFamily.GenericSansSerif)
                return "SansSerif";
            if (family == FontFamily.GenericSerif)
                return "Serif";
            if (family == FontFamily.GenericMonospace)
                return "Monospaced";
            return family.Name;
        }
        #endregion

        #region Colors
        private static Dictionary<string, Color> jtraingraphColors = new Dictionary<string, Color>()
        {
            ["schwarz"] = Color.Black,
            ["grau"] = Color.Gray,
            ["weiß"] = Color.White,
            ["rot"] = Color.Red,
            ["orange"] = Color.Orange,
            ["gelb"] = Color.Yellow,
            ["blau"] = Color.Blue,
            ["hellblau"] = Color.LightBlue,
            ["grün"] = Color.Green,
            ["dunkelgrün"] = Color.DarkGreen,
            ["braun"] = Color.Brown,
            ["magenta"] = Color.Magenta,
        };

        protected Color? ParseColor(string def, Color? defaultValue)
        {
            if (def == null)
                return defaultValue;

            if (def.StartsWith("#"))
                return ColorHelper.ColorFromHex(def);

            if (def.StartsWith("c(") && def.EndsWith(")"))
            {
                var parts = def.Substring(2, def.Length - 3).Split(',');
                return Color.FromArgb(int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2]));
            }

            if (jtraingraphColors.ContainsKey(def))
                return jtraingraphColors[def];

            return defaultValue;
        }

        protected Color ParseColor(string def, Color defaultValue)
            => ParseColor(def, new Color?(defaultValue)).Value;

        protected string ColorToString(Color color)
        {
            if (_parent.GetAttribute<string>("version") == "009")
                return ColorHelper.HexFromColor(color); // jTG 3.0

            return "c(" + color.R + "," + color.G + "," + color.B + ")"; // jTG 2.x
        }
        #endregion
    }
}

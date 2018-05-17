using System;
using System.Collections.Generic;
using System.Drawing;

namespace FPLedit.Shared.Helpers
{
    /// <summary>
    /// Konvertiert Farbangeben in das im Dateiformat übliche string-basierte Format.
    /// </summary>
    public static class ColorFormatter
    {
        #region Convert to string
        private static string ToHexString(Color c)
            => string.Format("#{0:X2}{1:X2}{2:X2}", c.R, c.G, c.B);

        private static string ToJtg2CustomColor(Color c)
            => "c(" + c.R + "," + c.G + "," + c.B + ")";

        public static string ToString(Color c, bool useJtg2Format = false)
            => useJtg2Format ? ToJtg2CustomColor(c) : ToHexString(c);
        #endregion

        #region Convert from string
        public static Color FromHexString(string hex)
        {
            if (hex.Length != 7 || hex[0] != '#')
                return Color.Empty;

            var b = Color.FromArgb(int.Parse(hex.Substring(1), System.Globalization.NumberStyles.HexNumber));
            return Color.FromArgb(255, b);
        }

        private static Color FromJtg2CustomColor(string jtg2)
        {
            var parts = jtg2.Substring(2, jtg2.Length - 3).Split(',');
            return Color.FromArgb(int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2]));
        }

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

        public static Color? FromString(string def, Color? defaultValue)
        {
            if (def == null)
                return defaultValue;

            if (def.StartsWith("#"))
                return FromHexString(def);

            if (def.StartsWith("c(") && def.EndsWith(")"))
                return FromJtg2CustomColor(def);

            if (jtraingraphColors.ContainsKey(def))
                return jtraingraphColors[def];

            return defaultValue;
        }

        public static Color FromString(string def, Color defaultValue)
            => FromString(def, new Color?(defaultValue)).Value;
        #endregion
    }
}

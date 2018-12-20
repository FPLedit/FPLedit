using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FPLedit.Shared.Rendering
{
    /// <summary>
    /// MetaFont to internally represent fonts.
    /// </summary>
    public class MFont
    {
        public MFont(string family, int size)
        {
            Family = family;
            Size = size;
        }

        public MFont(string family, int size, MFontStyle style) : this(family, size)
        {
            Style = style;
        }

        public string Family { get; set; }

        public int Size { get; set; }

        public MFontStyle Style { get; set; }

        public static explicit operator System.Drawing.Font(MFont m)
            => new System.Drawing.Font(m.Family, m.Size, (System.Drawing.FontStyle)m.Style);

        #region Conversion
        public static MFont Parse(string def)
        {
            if (def == null || !def.StartsWith("font(") || !def.EndsWith(")"))
                return new MFont("Arial", 9); // Keine valide Font-Definition

            var parts = def.Substring(5, def.Length - 6).Split(';');
            var family = GetFontFamily(parts[0]);
            var style = (MFontStyle)int.Parse(parts[1]);
            var size = int.Parse(parts[2]);

            return new MFont(family, size, style);
        }

        private static string GetFontFamily(string name)
        {
            switch (name.ToLower())
            {
                case "sansserif":
                case "dialog":
                case "dialoginput":
                    return System.Drawing.FontFamily.GenericSansSerif.Name;
                case "monospaced":
                    return System.Drawing.FontFamily.GenericMonospace.Name;
                case "serif":
                    return System.Drawing.FontFamily.GenericSerif.Name;
            }
            return name;
        }

        public string FontToString()
        {
            var family = GetFontName(Family);
            var style = (int)Style;
            return "font(" + family + ";" + style + ";" + Size + ")";
        }

        private string GetFontName(string family)
        {
            if (family == System.Drawing.FontFamily.GenericSansSerif.Name)
                return "SansSerif";
            if (family == System.Drawing.FontFamily.GenericSerif.Name)
                return "Serif";
            if (family == System.Drawing.FontFamily.GenericMonospace.Name)
                return "Monospaced";
            return family;
        }
        #endregion
    }

    [Flags]
    public enum MFontStyle
    {
        Regular = 0x0,
        Bold = 0x1,
        Italic = 0x2,
        Underline = 0x4,
        Strikeout = 0x8
    }
}

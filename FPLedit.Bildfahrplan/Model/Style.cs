using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using FPLedit.Shared.Helpers;

namespace FPLedit.Bildfahrplan.Model
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

        protected Color? ParseColor(string def, Color? defaultValue)
            => ColorFormatter.FromString(def, defaultValue);

        protected Color ParseColor(string def, Color defaultValue)
            => ColorFormatter.FromString(def, defaultValue);

        protected string ColorToString(Color color)
            => ColorFormatter.ToString(color, _parent.Version == TimetableVersion.JTG2_x);
    }
}

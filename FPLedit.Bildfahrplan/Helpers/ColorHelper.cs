using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace FPLedit.BildfahrplanExport
{
    public static class ColorHelper
    {
        public static Color FromHexString(string hex)
            => ColorTranslator.FromHtml(hex); //TODO: own translation

        public static Color FromJtg2CustomColor(string jtg2)
        {
            var parts = jtg2.Substring(2, jtg2.Length - 3).Split(',');
            return Color.FromArgb(int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2]));
        }

        public static string ToHexString(Color c)
            => string.Format("#{0:X2}{1:X2}{2:X2}", c.R, c.G, c.B);

        public static string ToJtg2CustomColor(Color c)
            => "c(" + c.R + "," + c.G + "," + c.B + ")";
    }
}

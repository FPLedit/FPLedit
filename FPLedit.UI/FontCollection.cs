using Eto.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPLedit.Shared.UI
{
    public static class FontCollection
    {
        public static string[] Families { get; private set; } = new string[0];

        //public static string GenericSansSerif { get; private set; } = "";

        //public static string GenericSerif { get; private set; } = "";

        //public static string GenericMonospace { get; private set; } = "";

        public static void InitAsync()
        {
            new Task(() =>
            {
                //Families = new InstalledFontCollection().Families.Select(f => f.Name).ToArray();
                Families = Fonts.AvailableFontFamilies.Select(f => f.Name).ToArray();
                //GenericSansSerif = FontFamily.GenericSansSerif.Name;
                //GenericSansSerif = FontFamily.GenericMonospace.Name;
                //GenericMonospace = FontFamily.GenericSerif.Name;
            }).Start();
        }
    }
}

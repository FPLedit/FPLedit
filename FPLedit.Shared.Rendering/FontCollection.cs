using Eto.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPLedit.Shared.Rendering
{
    public static class FontCollection
    {
        public static string[] Families { get; private set; } = Array.Empty<string>();

        public static string GenericSans { get; } = FontFamilies.SansFamilyName;

        public static string GenericSerif { get; } = FontFamilies.SerifFamilyName;

        public static string GenericMonospace { get; } = FontFamilies.MonospaceFamilyName;

        public static void InitAsync()
        {
            new Task(() =>
            {
                var installedFamilies = Fonts.AvailableFontFamilies.Select(f => f.Name).ToList();

                installedFamilies.InsertRange(0, new[] { GenericSans, GenericSerif, GenericMonospace });
                Families = installedFamilies.ToArray();
            }).Start();
        }
    }
}

using Eto.Drawing;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FPLedit.Shared.Rendering;

public static class FontCollection
{
    public static string[] Families { get; private set; } = Array.Empty<string>();

    public static string GenericSans => FontFamilies.SansFamilyName;

    public static string GenericSerif => FontFamilies.SerifFamilyName;

    public static string GenericMonospace => FontFamilies.MonospaceFamilyName;

    public static void InitAsync()
    {
        new Task(() =>
        {
            var installedFamilies = Fonts.AvailableFontFamilies.Select(f => f.Name).OrderBy(f => f).ToList();

            installedFamilies.InsertRange(0, new[] { GenericSans, GenericSerif, GenericMonospace });
            Families = installedFamilies.ToArray();
        }).Start();
    }
}
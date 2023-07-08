using System;
using FPLedit.Shared;
using FPLedit.Shared.Rendering;

namespace FPLedit.Bildfahrplan.Render;

#if ENABLE_SYSTEM_DRAWING
internal static class GdiAvailabilityTest
{
    private static bool Test()
    {
        try
        {
            using var img = MGraphicsSystemDrawing.CreateImage(1, 1, true);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static (string link, string description) GetPlatformGdiplusHint()
    {
        var (suffix, description) = Environment.OSVersion.Platform switch
        {
            PlatformID.Win32NT => ("install-windows/", T._("GDI+ steht nicht zur Verfügung. Das sollte niemals passieren?")),
            PlatformID.MacOSX => ("install-macos/", T._("Die Bibliothek libgdiplus wurde nicht gefunden!")),
            PlatformID.Unix => ("install-linux/", T._("Die Bibliothek libgdiplus wurde nicht gefunden!")),
            _ => ("", T._("Die Bibliothek libgdiplus wurde nicht gefunden!"))
        };
        return ($"https://fahrplan.manuelhu.de/download/{suffix}", description);
    }

    public static bool TestAndLog(IPluginInterface pluginInterface)
    {
        var testResult = Test();
        if (!testResult)
        {
            var (link, description) = GetPlatformGdiplusHint();
            pluginInterface.Logger.Error(T._("Die Bildfahrplanfunktionen stehen aufgrund eines Fehlers nicht zur Verfügung: {0} Zur Installation siehe Installationshinweise unter {1}.", description, link));
        }

        return testResult;
    }
}
#endif
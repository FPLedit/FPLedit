using System;

namespace FPLedit.Shared.Rendering;

/// <summary>
/// Helper class to get a <see cref="IMGraphics"/> instance with a backing image buffer.
/// </summary>
/// <remarks>Only use this if you don't care about the backing implementation.</remarks>
public static class MGraphics
{
    /// <summary>
    /// Create an image in ARGB with 32bpp depth with the specified dimensions.
    /// </summary>
    public static IMGraphics CreateImage(int width, int height, bool exportColor)
    {
#if ENABLE_SYSTEM_DRAWING
        bool useSystemDrawing = Environment.OSVersion.Platform == PlatformID.Win32NT || PreferSystemDrawing;
        if (useSystemDrawing)
            return MGraphicsSystemDrawing.CreateImage(width, height, exportColor);
#endif
        return MGraphicsImageSharp.CreateImage(width, height, exportColor);
    }

    public static bool PreferSystemDrawing { get; set; }
}
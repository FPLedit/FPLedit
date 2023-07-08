using System.Runtime.InteropServices;

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
    public static IMGraphics CreateImage(int width, int height)
    {
#if ENABLE_SYSTEM_DRAWING
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return MGraphicsSystemDrawing.CreateImage(width, height);
#endif
        return MGraphicsImageSharp.CreateImage(width, height);
    }
}
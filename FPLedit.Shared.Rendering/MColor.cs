using System;

namespace FPLedit.Shared.Rendering;

public sealed record MColor(byte R, byte G, byte B)
{
    public string Hex => ColorFormatter.ToString(this);

    public static explicit operator MColor(Eto.Drawing.Color sc)
        => new ((byte)sc.Rb, (byte)sc.Gb, (byte)sc.Bb);

    public static explicit operator Eto.Drawing.Color(MColor m)
        => Eto.Drawing.Color.FromArgb(m.R, m.G, m.B);

    public static explicit operator SixLabors.ImageSharp.Color(MColor m)
        => SixLabors.ImageSharp.Color.FromRgba(m.R, m.G, m.B, 255);

    public static explicit operator PdfSharp.Drawing.XColor(MColor m)
        => PdfSharp.Drawing.XColor.FromArgb(255, m.R, m.G, m.B);

#if ENABLE_SYSTEM_DRAWING
    public System.Drawing.Color ToSD() => System.Drawing.Color.FromArgb(255, R, G, B);
#endif

    public static MColor White => new (255, 255, 255);
}
using System;
using Eto.Drawing;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace FPLedit.Shared.Rendering;

/// <summary>
/// Konvertiert Farbangeben in das im Dateiformat übliche string-basierte Format.
/// </summary>
public static class ColorFormatter
{
    private static readonly char[] hexChars = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
        'A', 'B', 'C', 'D', 'E', 'F',
        'a', 'b', 'c', 'd', 'e', 'f' };

    #region Convert to string
    private static string ToHexString(MColor c)
        => $"#{c.R:X2}{c.G:X2}{c.B:X2}";

    private static string ToJtg2CustomColor(MColor c)
        => $"c({c.R},{c.G},{c.B})";

    public static string ToString(MColor c, bool useJtg2Format = false)
        => useJtg2Format ? ToJtg2CustomColor(c) : ToHexString(c);
    #endregion

    #region Convert from string
    public static MColor? FromHexString(string hex)
    {
        if (hex.Length != 7 || hex[0] != '#')
            return null;
        for (int i = 1; i< 7; i++)
            if (Array.IndexOf(hexChars, hex[i]) == -1)
                return null;

        var rgb = uint.Parse(hex[1..], System.Globalization.NumberStyles.HexNumber);
        return new((byte) (rgb >> 16), (byte) (rgb >> 8), (byte) rgb);
    }

    private static MColor FromJtg2CustomColor(string jtg2)
    {
        var parts = jtg2.Substring(2, jtg2.Length - 3).Split(',');
        return new MColor(byte.Parse(parts[0]), byte.Parse(parts[1]), byte.Parse(parts[2]));
    }

    private static readonly Dictionary<string, MColor> jtraingraphColors = new()
    {
        ["schwarz"]    = (MColor)Colors.Black,
        ["grau"]       = (MColor)Colors.Gray,
        ["weiß"]       = (MColor)Colors.White,
        ["rot"]        = (MColor)Colors.Red,
        ["orange"]     = (MColor)Colors.Orange,
        ["gelb"]       = (MColor)Colors.Yellow,
        ["blau"]       = (MColor)Colors.Blue,
        ["hellblau"]   = (MColor)Colors.LightBlue,
        ["grün"]       = (MColor)Colors.Green,
        ["dunkelgrün"] = (MColor)Colors.DarkGreen,
        ["braun"]      = (MColor)Colors.Brown,
        ["magenta"]    = (MColor)Colors.Magenta,
    };

    [return: NotNullIfNotNull("defaultValue")]
    public static MColor? FromString(string? def, MColor? defaultValue)
    {
        if (def == null)
            return defaultValue;

        if (def.StartsWith("#"))
            return FromHexString(def) ?? defaultValue;

        if (def.StartsWith("c(") && def.EndsWith(")"))
            return FromJtg2CustomColor(def);

        if (jtraingraphColors.TryGetValue(def, out var jtg))
            return jtg;

        return defaultValue;
    }
    #endregion
}
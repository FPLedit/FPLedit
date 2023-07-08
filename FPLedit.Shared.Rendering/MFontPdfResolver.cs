using System;
using System.IO;
using PdfSharp.Fonts;

namespace FPLedit.Shared.Rendering;

internal class MFontPdfResolver : IFontResolver
{
    public FontResolverInfo? ResolveTypeface(string familyName, bool isBold, bool isItalic)
    {
        var family = MFont.GetIsFontFamily(familyName);
        var style = (isBold ? SixLabors.Fonts.FontStyle.Bold : SixLabors.Fonts.FontStyle.Regular) & (isItalic ? SixLabors.Fonts.FontStyle.Italic : SixLabors.Fonts.FontStyle.Regular);
        var fnt = new SixLabors.Fonts.Font(family, 10, style);
        fnt.TryGetPath(out string? path);
        if (path == null)
            throw new InvalidOperationException("trying to use font without path!");
        return new FontResolverInfo(path);
    }

    public byte[]? GetFont(string faceName)
    {
        if (File.Exists(faceName))
            return File.ReadAllBytes(faceName);
        return null;
    }
}
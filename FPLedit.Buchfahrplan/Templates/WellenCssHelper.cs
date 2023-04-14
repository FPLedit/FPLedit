using System;
using System.IO;
using System.Text;
using Eto.Drawing;

namespace FPLedit.Buchfahrplan.Templates;

internal static class WellenCssHelper
{
    private static string? cache;
    private static bool usePngFallback;

    public static bool UsePngFallback
    {
        set
        {
            if (cache != null && value != usePngFallback) throw new InvalidOperationException("Cannot set png fallback rendering after initializing templates!");
            usePngFallback = value;
        }
    }

    private static string GetWelleSvg(int wl)
    {
        if (wl < 1 || wl > 3) throw new ArgumentException("Invaliud wl number", nameof(wl));

        var svg = $"<svg xmlns=\"http://www.w3.org/2000/svg\" height=\"4mm\" width=\"{2 * wl}mm\" version=\"1.1\" viewBox=\"0 0 {7 * wl} 14\">"
                  + "<g stroke=\"#000\" stroke-width=\"1px\" fill=\"none\">";
        for (int i = 0; i < wl; i++)
        {
            svg += $"<path d=\"m{i    *7} 7 7 7\"/>";
            svg += $"<path d=\"m{(i+1)*7} 0-7 7\"/>";
        }
        svg += "</g>";
        svg += "</svg>";

        var bytes = Encoding.UTF8.GetBytes(svg);
        return "data:image/svg+xml;base64," + Convert.ToBase64String(bytes);
    }

    private static string GetWellePng(int wl)
    {
        if (wl < 1 || wl > 3) throw new ArgumentException("Invaliud wl number", nameof(wl));

        const int sh = 32;
        using var bmp = new Bitmap(wl * sh, sh * 2, PixelFormat.Format32bppRgba);
        using (var pen = new Pen(Colors.Black, sh/8f))
        using (var g = new Graphics(bmp))
        {
            for (int i = 0; i < wl; i++)
            {
                g.DrawLine(pen, i*sh, sh, (i+1)*sh, 0);
                g.DrawLine(pen, i*sh, sh, (i+1)*sh, 2*sh);
            }
        }

        using (var ms = new MemoryStream())
        {
            bmp.Save(ms, ImageFormat.Png);
            ms.Seek(0, SeekOrigin.Begin);
            return "data:image/png;base64," + Convert.ToBase64String(ms.GetBuffer());
        }
    }

    public static string GetWellenCss()
    {
        Func<int,string> welle = usePngFallback ? GetWellePng : GetWelleSvg;
        return cache ??= ResourceHelper.GetStringResource("Buchfahrplan.Resources.WellenCss.css")
            .Replace("@@WELLE1@@", welle(1))
            .Replace("@@WELLE2@@", welle(2))
            .Replace("@@WELLE3@@", welle(3));
    }
}
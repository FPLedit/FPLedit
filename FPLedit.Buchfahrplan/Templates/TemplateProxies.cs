using System;
using System.Text;
using FPLedit.Shared.Templating;

namespace FPLedit.Buchfahrplan.Templates
{
    internal static class WellenCssHelper
    {
        private static string? cache;

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

        public static string GetWellenCss()
        {
            return cache ??= ResourceHelper.GetStringResource("Buchfahrplan.Resources.WellenCss.css")
                .Replace("@@WELLE1@@", GetWelleSvg(1))
                .Replace("@@WELLE2@@", GetWelleSvg(2))
                .Replace("@@WELLE3@@", GetWelleSvg(3));
        }
    }

    public sealed class StdTemplate : ITemplateProvider
    {
        public string TemplateIdentifier => "builtin:FPLedit.Buchfahrplan/Templates/StdTemplate.fpltmpl";

        public string GetTemplateCode() => ResourceHelper.GetStringResource("Buchfahrplan.Templates.StdTemplate.fpltmpl")
            .Replace("{{##WELLEN_CSS##}}", WellenCssHelper.GetWellenCss());
    }
    
    public sealed class ZlbTemplate : ITemplateProvider
    {
        public string TemplateIdentifier => "builtin:FPLedit.Buchfahrplan/Templates/ZlbTemplate.fpltmpl";

        public string GetTemplateCode() => ResourceHelper.GetStringResource("Buchfahrplan.Templates.ZlbTemplate.fpltmpl")
            .Replace("{{##WELLEN_CSS##}}", WellenCssHelper.GetWellenCss());
    }
}

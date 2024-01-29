using FPLedit.Buchfahrplan.Templates;
using FPLedit.Shared.Templating;

namespace FPLedit.Kursbuch.Templates;

internal sealed class TemplateProvider : ITemplateProvider
{
    internal const string IDENTFIER = "builtin:FPLedit.Kursbuch/Templates/KfplTemplate.fpltmpl";
    public string TemplateIdentifier => IDENTFIER;

    public string GetTemplateCode() => ResourceHelper.GetStringResource("Kursbuch.Templates.KfplTemplate.fpltmpl")
        .Replace("{{##WELLEN_CSS##}}", WellenCssHelper.GetWellenCss(1));
}
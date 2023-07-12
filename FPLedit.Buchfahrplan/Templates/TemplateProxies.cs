using FPLedit.Shared.Templating;

namespace FPLedit.Buchfahrplan.Templates;

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
using FPLedit.Shared.Templating;

namespace FPLedit.Kursbuch.Templates;

internal sealed class TemplateProvider : ITemplateProvider
{
    public string TemplateIdentifier => "builtin:FPLedit.Kursbuch/Templates/KfplTemplate.fpltmpl";

    public string GetTemplateCode() => ResourceHelper.GetStringResource("Kursbuch.Templates.KfplTemplate.fpltmpl");
}
using FPLedit.Shared;
using FPLedit.Shared.Templating;

namespace FPLedit.Aushangfahrplan.Templates;

internal sealed class StdTemplateProvider : BaseTemplateProxy, ITemplateProvider
{
    public string TemplateIdentifier => "builtin:FPLedit.Aushangfahrplan/Templates/AfplTemplate.fpltmpl";

    public string GetTemplateCode()
        => GeneratePreamble(T._("Nach Richtung sortiert, nach DV 450 der DRG"), "Abfahrt") +
           ResourceHelper.GetStringResource("Aushangfahrplan.Templates.AfplCommonDir.fpltmpl");
}

internal sealed class SvgTemplateProvider : BaseTemplateProxy, ITemplateProvider
{
    public string TemplateIdentifier => "builtin:FPLedit.Aushangfahrplan/Templates/SvgTemplate.fpltmpl";

    public string GetTemplateCode()
        => GeneratePreamble(T._(@"Nach Richtung sortiert, nach DV 450 der DRG, mit Schriftzug \""Abfahrt\"" in Originalschrift"), ResourceHelper.GetStringResource("Aushangfahrplan.Resources.abfahrt-text.svg")) +
           ResourceHelper.GetStringResource("Aushangfahrplan.Templates.AfplCommonDir.fpltmpl");
}

internal abstract class BaseTemplateProxy
{
    protected string GeneratePreamble(string name, string svg)
    {
        svg = svg.Replace("\\", "\\\\");
        svg = svg.Replace("\"", "\\\"");
        svg = svg.Replace("\n", "");
        svg = svg.Replace("\r", "");
        return $@"<#@ fpledit_template type=""afpl"" version=""2"" name=""{name}"" #>" + "\n" + $@"<# var abfahrtSVG = ""{svg}""; #>";
    }
}
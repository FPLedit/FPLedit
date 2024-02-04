using FPLedit.Shared;
using FPLedit.Shared.Templating;

namespace FPLedit.Aushangfahrplan.Templates;

internal sealed class StdTemplateProvider : BaseTemplateProxy, ITemplateProvider
{
    internal const string IDENTIFIER = "builtin:FPLedit.Aushangfahrplan/Templates/AfplTemplate.fpltmpl";
    public string TemplateIdentifier => IDENTIFIER;

    public string GetTemplateCode()
        => GeneratePreamble(T._("Standard (DRG aus Malsch)"), "Abfahrt") +
           ResourceHelper.GetStringResource("Aushangfahrplan.Templates.AfplCommon.fpltmpl");
}

internal sealed class SvgTemplateProvider : BaseTemplateProxy, ITemplateProvider
{
    public string TemplateIdentifier => "builtin:FPLedit.Aushangfahrplan/Templates/SvgTemplate.fpltmpl";

    public string GetTemplateCode()
        => GeneratePreamble(T._(@"Wie Standard, mit Schriftzug \""Abfahrt\"" in Originalschrift"), ResourceHelper.GetStringResource("Aushangfahrplan.Resources.abfahrt-text.svg")) +
           ResourceHelper.GetStringResource("Aushangfahrplan.Templates.AfplCommon.fpltmpl");
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
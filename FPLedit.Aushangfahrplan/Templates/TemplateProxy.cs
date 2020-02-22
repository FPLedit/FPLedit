using FPLedit.Shared.Templating;

namespace FPLedit.Aushangfahrplan.Templates
{
    internal class StdTemplateProxy : BaseTemplateProxy, ITemplateProxy
    {
        public string TemplateIdentifier => "builtin:FPLedit.Aushangfahrplan/Templates/AfplTemplate.fpltmpl";

        public string GetTemplateCode()
            => GeneratePreamble("Standard (DRG aus Malsch)", "Abfahrt") +
               ResourceHelper.GetStringResource("Aushangfahrplan.Templates.AfplCommon.fpltmpl");
    }

    internal class SvgTemplateProxy : BaseTemplateProxy, ITemplateProxy
    {
        public string TemplateIdentifier => "builtin:FPLedit.Aushangfahrplan/Templates/SvgTemplate.fpltmpl";

        public string GetTemplateCode()
            => GeneratePreamble(@"Wie Standard, mit Schriftzug \""Abfahrt\"" in Originalschrift", ResourceHelper.GetStringResource("Aushangfahrplan.Resources.abfahrt-text.svg")) + 
               ResourceHelper.GetStringResource("Aushangfahrplan.Templates.AfplCommon.fpltmpl");
    }

    internal class BaseTemplateProxy
    {
        protected string GeneratePreamble(string name, string svg)
        {
            svg = svg.Replace("\\", "\\\\");
            svg = svg.Replace("\"", "\\\"");
            svg = svg.Replace("\n", "");
            svg = svg.Replace("\r", "");
            return $@"<#@ fpledit-template type=""afpl"" version=""2"" name=""{name}"" #>" + "\n" + $@"<# var abfahrtSVG = ""{svg}""; #>";
        }
    }
}
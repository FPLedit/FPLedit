using FPLedit.Shared.Templating;
using System.IO;
using System.Reflection;

namespace FPLedit.Buchfahrplan.Templates
{
    public class StdTemplate : ITemplateProxy
    {
        public string TemplateIdentifier => "builtin:FPLedit.Buchfahrplan/Templates/StdTemplate.fpltmpl";

        public string GetTemplateCode() => ResourceHelper.GetStringResource("Buchfahrplan.Templates.StdTemplate.fpltmpl")
            .Replace("{{##WELLEN_CSS##}}", ResourceHelper.GetStringResource("Buchfahrplan.Resources.WellenCss.css"));

        public bool Javascript => true;
    }
    
    public class ZlbTemplate : ITemplateProxy
    {
        public string TemplateIdentifier => "builtin:FPLedit.Buchfahrplan/Templates/ZlbTemplate.fpltmpl";

        public string GetTemplateCode() => ResourceHelper.GetStringResource("Buchfahrplan.Templates.ZlbTemplate.fpltmpl")
            .Replace("{{##WELLEN_CSS##}}", ResourceHelper.GetStringResource("Buchfahrplan.Resources.WellenCss.css"));

        public bool Javascript => true;
    }
}

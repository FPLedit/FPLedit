using FPLedit.Shared.Templating;
using System.IO;
using System.Reflection;

namespace FPLedit.Kursbuch.Templates
{
    internal class TemplateProxy : ITemplateProxy
    {
        public string TemplateIdentifier => "builtin:FPLedit.Kursbuch/Templates/KfplTemplate.fpltmpl";

        public string GetTemplateCode() => ResourceHelper.GetStringResource("Kursbuch.Templates.KfplTemplate.fpltmpl");

        public bool Javascript => true;
    }
}
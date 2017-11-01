using FPLedit.Shared.Templating;
using System.IO;
using System.Reflection;

namespace FPLedit.Aushangfahrplan.Templates
{
    internal class StdTemplateProxy : ITemplateProxy
    {
        public string TemplateIdentifier => "builtin:FPLedit.Aushangfahrplan/Templates/AfplTemplate.fpltmpl";

        public string GetTemplateCode()
        {
            var a = Assembly.GetAssembly(GetType());
            string name = "FPLedit.Aushangfahrplan.Templates.AfplTemplate.fpltmpl";

            using (var stream = a.GetManifestResourceStream(name))
            using (var sr = new StreamReader(stream))
                return sr.ReadToEnd();
        }
    }

    internal class SvgTemplateProxy : ITemplateProxy
    {
        public string TemplateIdentifier => "builtin:FPLedit.Aushangfahrplan/Templates/SvgTemplate.fpltmpl";

        public string GetTemplateCode()
        {
            var a = Assembly.GetAssembly(GetType());
            string name = "FPLedit.Aushangfahrplan.Templates.SvgTemplate.fpltmpl";

            using (var stream = a.GetManifestResourceStream(name))
            using (var sr = new StreamReader(stream))
                return sr.ReadToEnd();
        }
    }
}
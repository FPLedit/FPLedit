using FPLedit.Shared.Templating;
using System.IO;
using System.Reflection;

namespace FPLedit.Aushangfahrplan.Templates
{
    internal class StdTemplateProxy : BaseTemplateProxy, ITemplateProxy
    {
        public string TemplateIdentifier => "builtin:FPLedit.Aushangfahrplan/Templates/AfplTemplate.fpltmpl";

        public string GetTemplateCode()
            => LoadFile("FPLedit.Aushangfahrplan.Templates.AfplTemplate.fpltmpl") +
            LoadFile("FPLedit.Aushangfahrplan.Templates.AfplCommon.fpltmpl");
        
        public bool Javascript => false;
    }

    internal class SvgTemplateProxy : BaseTemplateProxy, ITemplateProxy
    {
        public string TemplateIdentifier => "builtin:FPLedit.Aushangfahrplan/Templates/SvgTemplate.fpltmpl";

        public string GetTemplateCode()
            => LoadFile("FPLedit.Aushangfahrplan.Templates.SvgTemplate.fpltmpl") +
            LoadFile("FPLedit.Aushangfahrplan.Templates.AfplCommon.fpltmpl");
        
        public bool Javascript => false;
    }

    internal class BaseTemplateProxy
    {
        private Assembly assembly;

        protected string LoadFile(string dotPath)
        {
            if (assembly == null)
                assembly = Assembly.GetAssembly(GetType());

            using (var stream = assembly.GetManifestResourceStream(dotPath))
            using (var sr = new StreamReader(stream))
                return sr.ReadToEnd();
        }
    }
}
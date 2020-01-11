using FPLedit.Shared.Templating;
using System.IO;
using System.Reflection;

namespace FPLedit.Kursbuch.Templates
{
    internal class TemplateProxy : ITemplateProxy
    {
        public string TemplateIdentifier => "builtin:FPLedit.Kursbuch/Templates/KfplTemplate.fpltmpl";

        public string GetTemplateCode()
        {
            var a = Assembly.GetAssembly(GetType());
            string name = "FPLedit.Kursbuch.Templates.KfplTemplate.fpltmpl";

            using (var stream = a.GetManifestResourceStream(name))
            using (var sr = new StreamReader(stream))
                return sr.ReadToEnd();
        }

        public bool Javascript => true;
    }
}
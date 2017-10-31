using FPLedit.Shared.Templating;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace FPLedit.Buchfahrplan.Templates
{
    public class StdTemplate : ITemplateProxy
    {
        public string TemplateIdentifier => "builtin:FPLedit.Buchfahrplan/Templates/StdTemplate.tmpl";

        public string GetTemplateCode()
        {
            var a = Assembly.GetAssembly(GetType());
            string name = "FPLedit.Buchfahrplan.Templates.StdTemplate.fpltmpl";

            using (var stream = a.GetManifestResourceStream(name))
            using (var sr = new StreamReader(stream))
                return sr.ReadToEnd();
        }
    }

    public class ZlbTemplate : ITemplateProxy
    {
        public string TemplateIdentifier => "builtin:FPLedit.Buchfahrplan/Templates/ZlbTemplate.tmpl";

        public string GetTemplateCode()
        {
            var a = Assembly.GetAssembly(GetType());
            string name = "FPLedit.Buchfahrplan.Templates.ZlbTemplate.fpltmpl";

            using (var stream = a.GetManifestResourceStream(name))
            using (var sr = new StreamReader(stream))
                return sr.ReadToEnd();
        }
    }
}

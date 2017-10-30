using FPLedit.Shared.Templating;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace FPLedit.Buchfahrplan.Templates
{
    public class TestTemplate : ITemplateProxy
    {
        public string GetTemplateCode()
        {
            var a = Assembly.GetAssembly(GetType());
            string name = "FPLedit.Buchfahrplan.Templates.StdTemplate.tmpl";

            using (var stream = a.GetManifestResourceStream(name))
            using (var sr = new StreamReader(stream))
                return sr.ReadToEnd();
        }
    }
}

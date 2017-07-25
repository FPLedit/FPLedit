using FPLedit.Buchfahrplan.Model;
using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FPLedit.Buchfahrplan
{
    internal class BfplTemplateChooser
    {
        public BfplTemplateChooser(IInfo info)
        {
            AvailableTemplates = info.GetRegistered<IBfplTemplate>();
        }

        public IBfplTemplate GetTemplate(Timetable tt)
        {
            var attrsEn = tt.Children.FirstOrDefault(x => x.XName == "bfpl_attrs");

            var name = "";
            if (attrsEn != null)
            {
                var attrs = new BfplAttrs(attrsEn, tt);
                if (attrs.Template != "")
                    name = ExpandName(attrs.Template);
            }

            return AvailableTemplates.FirstOrDefault(t => t.GetType().FullName == name)
                ?? new Templates.BuchfahrplanTemplate();
        }

        public IBfplTemplate[] AvailableTemplates { get; private set; }

        public string ExpandName(string name)
            => name.Replace("$std", typeof(Templates.BuchfahrplanTemplate).Namespace);

        public string ReduceName(string name)
            => name.Replace(typeof(Templates.BuchfahrplanTemplate).Namespace, "$std");
    }
}

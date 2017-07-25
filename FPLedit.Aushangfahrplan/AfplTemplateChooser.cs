using FPLedit.Aushangfahrplan.Model;
using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FPLedit.Aushangfahrplan
{
    internal class AfplTemplateChooser
    {
        public AfplTemplateChooser(IInfo info)
        {
            AvailableTemplates = info.GetRegistered<IAfplTemplate>();
        }

        public IAfplTemplate GetTemplate(Timetable tt)
        {
            var attrsEn = tt.Children.FirstOrDefault(x => x.XName == "afpl_attrs");

            var name = "";
            if (attrsEn != null)
            {
                var attrs = new AfplAttrs(attrsEn, tt);
                if (attrs.Template != "")
                    name = ExpandName(attrs.Template);
            }

            return AvailableTemplates.FirstOrDefault(t => t.GetType().FullName == name)
                ?? new Templates.AfplTemplate();
        }

        public IAfplTemplate[] AvailableTemplates { get; private set; }

        public string ExpandName(string name)
            => name.Replace("$std", typeof(Templates.AfplTemplate).Namespace);

        public string ReduceName(string name)
            => name.Replace(typeof(Templates.AfplTemplate).Namespace, "$std");
    }
}

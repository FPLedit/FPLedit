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
        public IAfplTemplate GetTemplate(Timetable tt)
        {
            var attrsEn = tt.Children.FirstOrDefault(x => x.XName == "bfpl_attrs");

            var name = "";
            if (attrsEn != null)
            {
                var attrs = new AfplAttrs(attrsEn, tt);
                if (attrs.Template != "")
                    name = ExpandName(attrs.Template);
            }

            var templates = GetAvailableTemplates();

            return templates.FirstOrDefault(t => t.GetType().FullName == name)
                ?? new Templates.AfplTemplate();
        }

        public IAfplTemplate[] GetAvailableTemplates()
        {
            var orig_type = typeof(IAfplTemplate);

            var tmpls = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => orig_type.IsAssignableFrom(t) && orig_type != t)
                .Select(t => (IAfplTemplate)Activator.CreateInstance(t))
                .ToArray();

            return tmpls;
        }

        public string ExpandName(string name)
        {
            return name.Replace("$std", "FPLedit.AushangfahrplanExport.Templates");
        }

        public string ReduceName(string name)
        {
            return name.Replace("FPLedit.AushangfahrplanExport.Templates", "$std");
        }
    }
}

using FPLedit.Kursbuch.Model;
using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FPLedit.Kursbuch
{
    internal class AfplTemplateChooser
    {
        public AfplTemplateChooser(IInfo info)
        {
            AvailableTemplates = info.GetRegistered<IKfplTemplate>();
        }

        public IKfplTemplate GetTemplate(Timetable tt)
        {
            var attrsEn = tt.Children.FirstOrDefault(x => x.XName == "afpl_attrs");

            var name = "";
            if (attrsEn != null)
            {
                var attrs = new KfplAttrs(attrsEn, tt);
                if (attrs.Template != "")
                    name = ExpandName(attrs.Template);
            }

            return AvailableTemplates.FirstOrDefault(t => t.GetType().FullName == name)
                ?? new Templates.KfplTemplate();
        }

        public IKfplTemplate[] AvailableTemplates { get; private set; }

        public string ExpandName(string name)
            => name.Replace("$std", typeof(Templates.KfplTemplate).Namespace);

        public string ReduceName(string name)
            => name.Replace(typeof(Templates.KfplTemplate).Namespace, "$std");
    }
}

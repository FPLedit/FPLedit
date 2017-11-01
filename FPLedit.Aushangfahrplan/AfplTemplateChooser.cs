using FPLedit.Aushangfahrplan.Model;
using FPLedit.Shared;
using FPLedit.Shared.Templating;
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
            AvailableTemplates = info.TemplateManager.GetTemplates("afpl");
        }

        public ITemplate GetTemplate(Timetable tt)
        {
            var attrsEn = tt.Children.FirstOrDefault(x => x.XName == "afpl_attrs");

            var name = "";
            if (attrsEn != null)
            {
                var attrs = new AfplAttrs(attrsEn, tt);
                if (attrs.Template != "")
                    name = attrs.Template;
            }

            return GetTemplateByName(name) ??
                GetTemplateByName("builtin:FPLedit.Aushangfahrplan/Templates/AfplTemplate.fpltmpl");
        }

        private ITemplate GetTemplateByName(string name)
            => AvailableTemplates.FirstOrDefault(t => t.Identifier == name);

        public ITemplate[] AvailableTemplates { get; private set; }
    }
}

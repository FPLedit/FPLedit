using FPLedit.Kursbuch.Model;
using FPLedit.Shared;
using FPLedit.Shared.Templating;
using System.Linq;

namespace FPLedit.Kursbuch
{
    internal class KfplTemplateChooser
    {
        public KfplTemplateChooser(IInfo info)
        {
            AvailableTemplates = info.TemplateManager.GetTemplates("kfpl");
        }

        public ITemplate GetTemplate(Timetable tt)
        {
            var attrsEn = tt.Children.FirstOrDefault(x => x.XName == "kfpl_attrs");

            var name = "";
            if (attrsEn != null)
            {
                var attrs = new KfplAttrs(attrsEn, tt);
                if (attrs.Template != "")
                    name = attrs.Template;
            }

            return GetTemplateByName(name) ??
                GetTemplateByName("builtin:FPLedit.Kursbuch/Templates/KfplTemplate.fpltmpl");
        }

        private ITemplate GetTemplateByName(string name)
            => AvailableTemplates.FirstOrDefault(t => t.Identifier == name);

        public ITemplate[] AvailableTemplates { get; private set; }
    }
}

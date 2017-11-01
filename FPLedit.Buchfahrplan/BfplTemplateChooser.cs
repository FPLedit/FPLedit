using System.Linq;
using FPLedit.Buchfahrplan.Model;
using FPLedit.Shared;
using FPLedit.Shared.Templating;

namespace FPLedit.Buchfahrplan
{
    internal class BfplTemplateChooser
    {
        public BfplTemplateChooser(IInfo info)
        {
            AvailableTemplates = info.TemplateManager.GetTemplates("bfpl");
        }

        public ITemplate GetTemplate(Timetable tt)
        {
            var attrsEn = tt.Children.FirstOrDefault(x => x.XName == "bfpl_attrs");

            var name = "";
            if (attrsEn != null)
            {
                var attrs = new BfplAttrs(attrsEn, tt);
                if (attrs.Template != "")
                    name = attrs.Template;
            }

            return GetTemplateByName(name) ??
                GetTemplateByName("builtin:FPLedit.Buchfahrplan/Templates/StdTemplate.fpltmpl");
        }

        private ITemplate GetTemplateByName(string name)
            => AvailableTemplates.FirstOrDefault(t => t.Identifier == name);

        public ITemplate[] AvailableTemplates { get; private set; }
    }
}

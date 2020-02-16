using System.Linq;
using FPLedit.Buchfahrplan.Model;
using FPLedit.Shared;
using FPLedit.Shared.Templating;

namespace FPLedit.Buchfahrplan
{
    internal class BfplTemplateChooser : BaseTemplateChooser
    {
        protected override string DefaultTemplate => "builtin:FPLedit.Buchfahrplan/Templates/StdTemplate.fpltmpl";
        protected override string ElemName => "bfpl_attrs";
        protected override string AttrName => "tmpl";

        public BfplTemplateChooser(IReducedPluginInterface pluginInterface) : base("bfpl", pluginInterface)
        {
        }
    }
}

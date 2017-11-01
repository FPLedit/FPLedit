using FPLedit.Shared;
using FPLedit.Shared.Templating;

namespace FPLedit.Aushangfahrplan
{
    internal class AfplTemplateChooser : BaseTemplateChooser
    {
        protected override string DefaultTemplate => "builtin:FPLedit.Aushangfahrplan/Templates/AfplTemplate.fpltmpl";
        protected override string ElemName => "afpl_attrs";
        protected override string AttrName => "tmpl";

        public AfplTemplateChooser(IInfo info) : base("afpl", info)
        {
        }
    }
}

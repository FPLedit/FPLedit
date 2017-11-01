using FPLedit.Shared;
using FPLedit.Shared.Templating;

namespace FPLedit.Kursbuch
{
    internal class KfplTemplateChooser : BaseTemplateChooser
    {
        protected override string DefaultTemplate => "builtin:FPLedit.Kursbuch/Templates/KfplTemplate.fpltmpl";
        protected override string ElemName => "kfpl_attrs";
        protected override string AttrName => "tmpl";

        public KfplTemplateChooser(IInfo info) : base("kfpl", info)
        {
        }
    }
}

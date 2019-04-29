using System.Linq;
using FPLedit.Kursbuch.Forms;
using FPLedit.Kursbuch.Templates;
using FPLedit.Shared;
using FPLedit.Shared.Filetypes;
using FPLedit.Shared.Templating;
using FPLedit.Shared.Ui;

namespace FPLedit.Kursbuch
{
    [Plugin("Modul für Tabellenfahrpläne", Vi.PFrom, Vi.PUpTo, Author = "Manuel Huber")]
    public class Plugin : IPlugin
    {
        public void Init(IInfo info)
        {
            info.Register<IExport>(new HtmlExport());
            info.Register<IPreviewable>(new Preview());
            info.Register<IFilterableUi>(new FilterableHandler());
            info.Register<IDesignableUiProxy>(new SettingsControlProxy());

            info.Register<ITemplateProxy>(new TemplateProxy());

            info.Register<ITimetableTypeChangeAction>(new FixAttrsAction());
        }
    }

    public class FixAttrsAction : BaseConverterFileType, ITimetableTypeChangeAction
    {
        public void ToLinear(Timetable tt)
        {
            var attrs = Model.KfplAttrs.GetAttrs(tt);
            if (attrs == null)
                return;
            var route = tt.GetRoutes().FirstOrDefault().Index;
            ConvertAttrNetToLin(attrs.KBSn, route);
        }

        public void ToNetwork(Timetable tt)
        {
            var attrs = Model.KfplAttrs.GetAttrs(tt);
            if (attrs == null)
                return;
            ConvertAttrLinToNet(attrs.KBSn);
        }
    }
}

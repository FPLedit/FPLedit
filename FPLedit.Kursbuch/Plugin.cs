using System.Collections.Generic;
using System.Linq;
using FPLedit.Kursbuch.Forms;
using FPLedit.Shared;
using FPLedit.Shared.Filetypes;
using FPLedit.Shared.Templating;
using FPLedit.Shared.Ui;

namespace FPLedit.Kursbuch
{
    [Plugin("Modul für Tabellenfahrpläne", Vi.PFrom, Vi.PUpTo, Author = "Manuel Huber")]
    public class Plugin : IPlugin
    {
        public void Init(IPluginInterface pluginInterface)
        {
            pluginInterface.Register<IExport>(new HtmlExport());
            pluginInterface.Register<IPreviewable>(new Preview());
            pluginInterface.Register<IFilterableUi>(new FilterableHandler());
            pluginInterface.Register<IDesignableUiProxy>(new SettingsControlProxy());

            pluginInterface.Register<ITemplateProxy>(new Templates.TemplateProxy());
            
            pluginInterface.Register<ITemplateWhitelist>(new TemplateWhitelist<Templates.TemplateHelper>("kfpl"));
            pluginInterface.Register<ITemplateWhitelist>(new TemplateWhitelist<Model.KfplAttrs>("kfpl"));

            pluginInterface.Register<ITimetableTypeChangeAction>(new FixAttrsAction());
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

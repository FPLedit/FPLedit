using FPLedit.Aushangfahrplan.Forms;
using FPLedit.Shared;
using FPLedit.Shared.Templating;
using FPLedit.Shared.Ui;

namespace FPLedit.Aushangfahrplan
{
    [Plugin("Modul für Aushangfahrpläne", Vi.PFrom, Vi.PUpTo, Author = "Manuel Huber")]
    public class Plugin : IPlugin
    {
        public void Init(IInfo info)
        {
            info.Register<IExport>(new HtmlExport());
            info.Register<IDesignableUiProxy>(new SettingsControlProxy());
            info.Register<IFilterableUi>(new FilterableHandler());
            info.Register<IPreviewable>(new Preview());

            info.Register<ITemplateProxy>(new Templates.StdTemplateProxy());
            info.Register<ITemplateProxy>(new Templates.SvgTemplateProxy());
        }
    }
}

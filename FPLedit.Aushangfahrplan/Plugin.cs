using FPLedit.Aushangfahrplan.Forms;
using FPLedit.Shared;
using FPLedit.Shared.Templating;
using FPLedit.Shared.Ui;

namespace FPLedit.Aushangfahrplan
{
    [Plugin("Modul für Aushangfahrpläne", "1.5.0", "2.0", Author = "Manuel Huber")]
    public class Plugin : IPlugin
    {
        private IInfo info;

        public void Init(IInfo info)
        {
            this.info = info;

            info.Register<IExport>(new HtmlExport());
            info.Register<IDesignableUiProxy>(new SettingsControlProxy());
            info.Register<IFilterableUi>(new FilterableHandler());
            info.Register<IPreviewable>(new Preview());

            info.Register<ITemplateProxy>(new Templates.StdTemplateProxy());
            info.Register<ITemplateProxy>(new Templates.SvgTemplateProxy());
        }
    }
}

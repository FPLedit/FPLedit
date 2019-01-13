using FPLedit.Kursbuch.Forms;
using FPLedit.Kursbuch.Templates;
using FPLedit.Shared;
using FPLedit.Shared.Templating;
using FPLedit.Shared.Ui;

namespace FPLedit.Kursbuch
{
    [Plugin("Modul für Tabellenfahrpläne", Vi.PFrom, Vi.PUpTo, Author = "Manuel Huber")]
    public class Plugin : IPlugin
    {
        private IInfo info;

        public void Init(IInfo info)
        {
            this.info = info;

            info.Register<IExport>(new HtmlExport());
            info.Register<IPreviewable>(new Preview());
            info.Register<IFilterableUi>(new FilterableHandler());
            info.Register<IDesignableUiProxy>(new SettingsControlProxy());

            info.Register<ITemplateProxy>(new TemplateProxy());
        }
    }
}

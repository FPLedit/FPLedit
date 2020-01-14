using FPLedit.Aushangfahrplan.Forms;
using FPLedit.Shared;
using FPLedit.Shared.Templating;
using FPLedit.Shared.Ui;

namespace FPLedit.Aushangfahrplan
{
    [Plugin("Modul für Aushangfahrpläne", Vi.PFrom, Vi.PUpTo, Author = "Manuel Huber")]
    public class Plugin : IPlugin
    {
        public void Init(IPluginInterface pluginInterface)
        {
            pluginInterface.Register<IExport>(new HtmlExport());
            pluginInterface.Register<IDesignableUiProxy>(new SettingsControlProxy());
            pluginInterface.Register<IFilterableUi>(new FilterableHandler());
            pluginInterface.Register<IPreviewable>(new Preview());

            pluginInterface.Register<ITemplateProxy>(new Templates.StdTemplateProxy());
            pluginInterface.Register<ITemplateProxy>(new Templates.SvgTemplateProxy());
            
            pluginInterface.Register<ITemplateWhitelist>(new TemplateWhitelist<Templates.TemplateHelper>("afpl"));
            pluginInterface.Register<ITemplateWhitelist>(new TemplateWhitelist<Model.AfplAttrs>("afpl"));
        }
    }
}

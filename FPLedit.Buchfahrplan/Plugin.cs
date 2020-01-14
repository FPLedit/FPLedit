using FPLedit.Shared;
using FPLedit.Shared.Templating;
using FPLedit.Shared.Ui;
using System;

namespace FPLedit.Buchfahrplan
{
    [Plugin("Modul für Buchfahrpläne", Vi.PFrom, Vi.PUpTo, Author = "Manuel Huber")]
    public class Plugin : IPlugin
    {
        public void Init(IPluginInterface pluginInterface)
        {
            pluginInterface.Register<IExport>(new HtmlExport());
            pluginInterface.Register<IDesignableUiProxy>(new SettingsControlProxy());
            pluginInterface.Register<IFilterableUi>(new Forms.FilterableHandler());
            pluginInterface.Register<IPreviewable>(new Forms.Preview());
            pluginInterface.Register<IRouteAction>(new Forms.VelocityDialogProxy());

            pluginInterface.Register<ITemplateProxy>(new Templates.StdTemplate());
            pluginInterface.Register<ITemplateProxy>(new Templates.ZlbTemplate());
            
            pluginInterface.Register<ITemplateWhitelist>(new TemplateWhitelist<Templates.TemplateHelper>("bfpl"));
            pluginInterface.Register<ITemplateWhitelist>(new TemplateWhitelist<Model.BfplAttrs>("bfpl"));
            pluginInterface.Register<ITemplateWhitelist>(new TemplateWhitelist<Model.BfplPoint>("bfpl"));
        }
    }
}

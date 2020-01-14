using FPLedit.Shared;
using FPLedit.Shared.Templating;
using FPLedit.Shared.Ui;
using System;

namespace FPLedit.Buchfahrplan
{
    [Plugin("Modul für Buchfahrpläne", Vi.PFrom, Vi.PUpTo, Author = "Manuel Huber")]
    public class Plugin : IPlugin
    {
        public void Init(IInfo info)
        {
            info.Register<IExport>(new HtmlExport());
            info.Register<IDesignableUiProxy>(new SettingsControlProxy());
            info.Register<IFilterableUi>(new Forms.FilterableHandler());
            info.Register<IPreviewable>(new Forms.Preview());
            info.Register<IRouteAction>(new Forms.VelocityDialogProxy());

            info.Register<ITemplateProxy>(new Templates.StdTemplate());
            info.Register<ITemplateProxy>(new Templates.ZlbTemplate());
            
            info.Register<ITemplateWhitelist>(new TemplateWhitelist<Templates.TemplateHelper>("bfpl"));
            info.Register<ITemplateWhitelist>(new TemplateWhitelist<Model.BfplAttrs>("bfpl"));
            info.Register<ITemplateWhitelist>(new TemplateWhitelist<Model.BfplPoint>("bfpl"));
        }
    }
}

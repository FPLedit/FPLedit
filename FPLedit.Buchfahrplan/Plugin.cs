using FPLedit.Shared;
using FPLedit.Shared.Templating;
using FPLedit.Shared.Ui;
using System;

namespace FPLedit.Buchfahrplan
{
    [Plugin("Modul für Buchfahrpläne", Pvi.From, Pvi.UpTo, Author = "Manuel Huber")]
    public class Plugin : IPlugin
    {
        private IInfo info;

        public void Init(IInfo info)
        {
            this.info = info;

            info.Register<IExport>(new HtmlExport());
            info.Register<IDesignableUiProxy>(new SettingsControlProxy());
            info.Register<IFilterableUi>(new Forms.FilterableHandler());
            info.Register<IPreviewable>(new Forms.Preview());
            info.Register<IRouteAction>(new Forms.VelocityDialogProxy());

            info.Register<ITemplateProxy>(new Templates.StdTemplate());
            info.Register<ITemplateProxy>(new Templates.ZlbTemplate());
        }
    }
}

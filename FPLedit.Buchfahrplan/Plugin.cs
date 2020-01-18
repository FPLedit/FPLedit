using FPLedit.Shared;
using FPLedit.Shared.Templating;
using System;
using FPLedit.Buchfahrplan.Model;
using FPLedit.Shared.DefaultImplementations;

namespace FPLedit.Buchfahrplan
{
    [Plugin("Modul für Buchfahrpläne", Vi.PFrom, Vi.PUpTo, Author = "Manuel Huber")]
    public class Plugin : IPlugin
    {
        public void Init(IPluginInterface pluginInterface)
        {
            var export = new BasicTemplateExport("Buchfahrplan als HTML Datei (*.html)|*.html", pi => new BfplTemplateChooser(pi));
            var preview = new BasicPreview("bfpl", "Buchfahrplan", export);
            pluginInterface.Register<IExport>(export);
            pluginInterface.Register<IPreviewProxy>(preview);
            
            pluginInterface.Register<IAppearanceControl>(new SettingsControlProxy());
            pluginInterface.Register<IFilterableProvider>(new BasicFilterableProvider("Buchfahrplan", BfplAttrs.GetAttrs, BfplAttrs.CreateAttrs));
            pluginInterface.Register<IRouteAction>(new Forms.VelocityDialogProxy());

            pluginInterface.Register<ITemplateProxy>(new Templates.StdTemplate());
            pluginInterface.Register<ITemplateProxy>(new Templates.ZlbTemplate());
            
            pluginInterface.Register<ITemplateWhitelist>(new TemplateWhitelist<Templates.TemplateHelper>("bfpl"));
            pluginInterface.Register<ITemplateWhitelist>(new TemplateWhitelist<Model.BfplAttrs>("bfpl"));
            pluginInterface.Register<ITemplateWhitelist>(new TemplateWhitelist<Model.BfplPoint>("bfpl"));
        }
    }
}

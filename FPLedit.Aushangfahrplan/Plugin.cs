using FPLedit.Aushangfahrplan.Forms;
using FPLedit.Aushangfahrplan.Model;
using FPLedit.Shared;
using FPLedit.Shared.DefaultImplementations;
using FPLedit.Shared.Templating;

namespace FPLedit.Aushangfahrplan
{
    [Plugin("Modul für Aushangfahrpläne", Vi.PFrom, Vi.PUpTo, Author = "Manuel Huber")]
    public class Plugin : IPlugin
    {
        public void Init(IPluginInterface pluginInterface)
        {
            var export = new BasicTemplateExport("Aushangfahrplan HTML Datei (*.html)|*.html", pi => new AfplTemplateChooser(pi));
            var preview = new BasicPreview("afpl", "Aushangfahrplan", export);
            pluginInterface.Register<IExport>(export);
            pluginInterface.Register<IPreviewProxy>(preview);
            
            pluginInterface.Register<IAppearanceControl>(new SettingsControlProxy());
            pluginInterface.Register<IFilterableProvider>(new BasicFilterableProvider("Aushangfahrplan", AfplAttrs.GetAttrs, AfplAttrs.CreateAttrs));

            pluginInterface.Register<ITemplateProxy>(new Templates.StdTemplateProxy());
            pluginInterface.Register<ITemplateProxy>(new Templates.SvgTemplateProxy());
            
            pluginInterface.Register<ITemplateWhitelist>(new TemplateWhitelist<Templates.TemplateHelper>("afpl"));
            pluginInterface.Register<ITemplateWhitelist>(new TemplateWhitelist<Model.AfplAttrs>("afpl"));
        }
    }
}

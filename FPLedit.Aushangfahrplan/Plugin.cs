using FPLedit.Aushangfahrplan.Forms;
using FPLedit.Aushangfahrplan.Model;
using FPLedit.Shared;
using FPLedit.Shared.DefaultImplementations;
using FPLedit.Shared.Templating;

namespace FPLedit.Aushangfahrplan
{
    [Plugin("Modul für Aushangfahrpläne", Vi.PFrom, Vi.PUpTo, Author = "Manuel Huber")]
    public sealed class Plugin : IPlugin
    {
        public void Init(IPluginInterface pluginInterface)
        {
            var export = new BasicTemplateExport("Aushangfahrplan HTML Datei (*.html)|*.html", GetTemplateChooser);
            var preview = new BasicPreview("afpl", "Aushangfahrplan", export);
            pluginInterface.Register<IExport>(export);
            pluginInterface.Register<IPreviewProxy>(preview);
            
            pluginInterface.Register<IAppearanceControl>(new BasicAppearanceControl(pi => new SettingsControl(pi), "Aushangfahrplan"));
            pluginInterface.Register<IFilterableProvider>(FilterableProvider);

            pluginInterface.Register<ITemplateProxy>(new Templates.StdTemplateProxy());
            pluginInterface.Register<ITemplateProxy>(new Templates.SvgTemplateProxy());
            
            pluginInterface.Register<ITemplateWhitelist>(new TemplateWhitelist<Templates.TemplateHelper>("afpl"));
            pluginInterface.Register<ITemplateWhitelist>(new TemplateWhitelist<Model.AfplAttrs>("afpl"));
        }
        
        internal static IFilterableProvider FilterableProvider => new BasicFilterableProvider("Aushangfahrplan", AfplAttrs.GetAttrs, AfplAttrs.CreateAttrs);
        
        internal static ITemplateChooser GetTemplateChooser(IReducedPluginInterface pi) 
            => new BasicTemplateChooser(pi, "afpl", "afpl_attrs", "tmpl", "builtin:FPLedit.Aushangfahrplan/Templates/AfplTemplate.fpltmpl");
    }
}

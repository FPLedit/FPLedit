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
        public void Init(IPluginInterface pluginInterface, IComponentRegistry componentRegistry)
        {
            var export = new DefaultTemplateExport("Aushangfahrplan HTML Datei (*.html)|*.html", GetTemplateChooser);
            var preview = new DefaultPreview("afpl", "Aushangfahrplan", export);
            componentRegistry.Register<IExport>(export);
            componentRegistry.Register<IPreviewAction>(preview);
            
            componentRegistry.Register<IAppearanceControl>(new DefaultAppearanceControl(pi => new SettingsControl(pi), "Aushangfahrplan"));
            componentRegistry.Register<IFilterRuleContainer>(FilterRuleContainer);

            componentRegistry.Register<ITemplateProvider>(new Templates.StdTemplateProvider());
            componentRegistry.Register<ITemplateProvider>(new Templates.SvgTemplateProvider());
            
            componentRegistry.Register<ITemplateWhitelistEntry>(new TemplateWhitelistEntry<Templates.TemplateHelper>("afpl"));
            componentRegistry.Register<ITemplateWhitelistEntry>(new TemplateWhitelistEntry<Model.AfplAttrs>("afpl"));
        }
        
        internal static IFilterRuleContainer FilterRuleContainer => new DefaultFilterRuleContainer("Aushangfahrplan", AfplAttrs.GetAttrs, AfplAttrs.CreateAttrs);
        
        internal static ITemplateChooser GetTemplateChooser(IReducedPluginInterface pi) 
            => new DefaultTemplateChooser(pi, "afpl", "afpl_attrs", "tmpl", "builtin:FPLedit.Aushangfahrplan/Templates/AfplTemplate.fpltmpl");
    }
}

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
            var export = new DefaultTemplateExport("Aushangfahrplan HTML Datei (*.html)|*.html", GetTemplateChooser);
            var preview = new DefaultPreview("afpl", "Aushangfahrplan", export);
            pluginInterface.Register<IExport>(export);
            pluginInterface.Register<IPreviewAction>(preview);
            
            pluginInterface.Register<IAppearanceControl>(new DefaultAppearanceControl(pi => new SettingsControl(pi), "Aushangfahrplan"));
            pluginInterface.Register<IFilterRuleContainer>(FilterRuleContainer);

            pluginInterface.Register<ITemplateProvider>(new Templates.StdTemplateProvider());
            pluginInterface.Register<ITemplateProvider>(new Templates.SvgTemplateProvider());
            
            pluginInterface.Register<ITemplateWhitelistEntry>(new TemplateWhitelistEntry<Templates.TemplateHelper>("afpl"));
            pluginInterface.Register<ITemplateWhitelistEntry>(new TemplateWhitelistEntry<Model.AfplAttrs>("afpl"));
        }
        
        internal static IFilterRuleContainer FilterRuleContainer => new DefaultFilterRuleContainer("Aushangfahrplan", AfplAttrs.GetAttrs, AfplAttrs.CreateAttrs);
        
        internal static ITemplateChooser GetTemplateChooser(IReducedPluginInterface pi) 
            => new DefaultTemplateChooser(pi, "afpl", "afpl_attrs", "tmpl", "builtin:FPLedit.Aushangfahrplan/Templates/AfplTemplate.fpltmpl");
    }
}

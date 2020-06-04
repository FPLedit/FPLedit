using FPLedit.Shared;
using FPLedit.Shared.Templating;
using FPLedit.Buchfahrplan.Model;
using FPLedit.Shared.DefaultImplementations;

namespace FPLedit.Buchfahrplan
{
    [Plugin("Modul für Buchfahrpläne", Vi.PFrom, Vi.PUpTo, Author = "Manuel Huber")]
    public sealed class Plugin : IPlugin
    {
        public void Init(IPluginInterface pluginInterface, IComponentRegistry componentRegistry)
        {
            var export = new DefaultTemplateExport("Buchfahrplan als HTML Datei (*.html)|*.html", GetTemplateChooser);
            var preview = new DefaultPreview("bfpl", "Buchfahrplan", export);
            componentRegistry.Register<IExport>(export);
            componentRegistry.Register<IPreviewAction>(preview);
            
            componentRegistry.Register<IAppearanceControl>(new DefaultAppearanceControl(pi => new Forms.SettingsControl(pi), "Buchfahrplan"));
            componentRegistry.Register<IFilterRuleContainer>(FilterRuleContainer);
            componentRegistry.Register<IRouteAction>(new Forms.VelocityRouteAction());

            componentRegistry.Register<ITemplateProvider>(new Templates.StdTemplate());
            componentRegistry.Register<ITemplateProvider>(new Templates.ZlbTemplate());
            
            componentRegistry.Register<ITemplateWhitelistEntry>(new TemplateWhitelistEntry<Templates.TemplateHelper>("bfpl"));
            componentRegistry.Register<ITemplateWhitelistEntry>(new TemplateWhitelistEntry<Model.BfplAttrs>("bfpl"));
            componentRegistry.Register<ITemplateWhitelistEntry>(new TemplateWhitelistEntry<Model.BfplPoint>("bfpl"));
        }
        
        internal static IFilterRuleContainer FilterRuleContainer => new DefaultFilterRuleContainer("Buchfahrplan", BfplAttrs.GetAttrs, BfplAttrs.CreateAttrs);
        
        internal static ITemplateChooser GetTemplateChooser(IReducedPluginInterface pi) 
            => new DefaultTemplateChooser(pi, "bfpl", "bfpl_attrs", "tmpl", "builtin:FPLedit.Buchfahrplan/Templates/StdTemplate.fpltmpl");
    }
}

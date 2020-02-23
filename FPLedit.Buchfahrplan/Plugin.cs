using FPLedit.Shared;
using FPLedit.Shared.Templating;
using System;
using FPLedit.Buchfahrplan.Model;
using FPLedit.Shared.DefaultImplementations;

namespace FPLedit.Buchfahrplan
{
    [Plugin("Modul für Buchfahrpläne", Vi.PFrom, Vi.PUpTo, Author = "Manuel Huber")]
    public sealed class Plugin : IPlugin
    {
        public void Init(IPluginInterface pluginInterface)
        {
            var export = new DefaultTemplateExport("Buchfahrplan als HTML Datei (*.html)|*.html", GetTemplateChooser);
            var preview = new DefaultPreview("bfpl", "Buchfahrplan", export);
            pluginInterface.Register<IExport>(export);
            pluginInterface.Register<IPreviewAction>(preview);
            
            pluginInterface.Register<IAppearanceControl>(new DefaultAppearanceControl(pi => new Forms.SettingsControl(pi), "Buchfahrplan"));
            pluginInterface.Register<IFilterRuleContainer>(FilterRuleContainer);
            pluginInterface.Register<IRouteAction>(new Forms.VelocityRouteAction());

            pluginInterface.Register<ITemplateProvider>(new Templates.StdTemplate());
            pluginInterface.Register<ITemplateProvider>(new Templates.ZlbTemplate());
            
            pluginInterface.Register<ITemplateWhitelistEntry>(new TemplateWhitelistEntry<Templates.TemplateHelper>("bfpl"));
            pluginInterface.Register<ITemplateWhitelistEntry>(new TemplateWhitelistEntry<Model.BfplAttrs>("bfpl"));
            pluginInterface.Register<ITemplateWhitelistEntry>(new TemplateWhitelistEntry<Model.BfplPoint>("bfpl"));
        }
        
        internal static IFilterRuleContainer FilterRuleContainer => new DefaultFilterRuleContainer("Buchfahrplan", BfplAttrs.GetAttrs, BfplAttrs.CreateAttrs);
        
        internal static ITemplateChooser GetTemplateChooser(IReducedPluginInterface pi) 
            => new DefaultTemplateChooser(pi, "bfpl", "bfpl_attrs", "tmpl", "builtin:FPLedit.Buchfahrplan/Templates/StdTemplate.fpltmpl");
    }
}

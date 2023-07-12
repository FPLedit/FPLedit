using FPLedit.Aushangfahrplan.Forms;
using FPLedit.Aushangfahrplan.Model;
using FPLedit.Shared;
using FPLedit.Shared.DefaultImplementations;
using FPLedit.Shared.Templating;

namespace FPLedit.Aushangfahrplan;

[Plugin("Modul für Aushangfahrpläne", Vi.PFrom, Vi.PUpTo, Author = "Manuel Huber")]
public sealed class Plugin : IPlugin, ITemplatePlugin
{
    public void Init(IPluginInterface pluginInterface, IComponentRegistry componentRegistry)
    {
        var export = new DefaultTemplateExport(T._("Aushangfahrplan HTML Datei (*.html)|*.html"), GetTemplateChooser);
        var preview = new DefaultPreview("afpl", T._("Aushangfahrplan"), export);
        componentRegistry.Register<IExport>(export);
        componentRegistry.Register<IPreviewAction>(preview);
            
        componentRegistry.Register<IAppearanceControl>(new DefaultAppearanceControl(pi => new SettingsControl(pi), T._("Aushangfahrplan")));
        componentRegistry.Register<IFilterRuleContainer>(FilterRuleContainer);
            
        InitTemplates(pluginInterface, componentRegistry);
    }

    public void InitTemplates(IPluginInterface pluginInterface, IComponentRegistry componentRegistry)
    {
        componentRegistry.Register<ITemplateProvider>(new Templates.StdTemplateProvider());
        componentRegistry.Register<ITemplateProvider>(new Templates.SvgTemplateProvider());
            
        componentRegistry.Register<ITemplateWhitelistEntry>(new TemplateWhitelistEntry<Templates.TemplateHelper>("afpl"));
        componentRegistry.Register<ITemplateWhitelistEntry>(new TemplateWhitelistEntry<AfplAttrs>("afpl"));
    }
        
    internal static IFilterRuleContainer FilterRuleContainer => new DefaultFilterRuleContainer(T._("Aushangfahrplan"), AfplAttrs.GetAttrs, AfplAttrs.CreateAttrs);
        
    internal static ITemplateChooser GetTemplateChooser(IReducedPluginInterface pi) 
        => new DefaultTemplateChooser(pi, "afpl", "afpl_attrs", "tmpl", "builtin:FPLedit.Aushangfahrplan/Templates/AfplTemplate.fpltmpl");
}
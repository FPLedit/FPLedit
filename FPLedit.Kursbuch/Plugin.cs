using System.Linq;
using FPLedit.Kursbuch.Forms;
using FPLedit.Kursbuch.Model;
using FPLedit.Shared;
using FPLedit.Shared.DefaultImplementations;
using FPLedit.Shared.Filetypes;
using FPLedit.Shared.Templating;

namespace FPLedit.Kursbuch;

[Plugin("Modul für Tabellenfahrpläne", Vi.PFrom, Vi.PUpTo, Author = "Manuel Huber")]
public sealed class Plugin : IPlugin, ITemplatePlugin
{
    public void Init(IPluginInterface pluginInterface, IComponentRegistry componentRegistry)
    {
        var export = new DefaultTemplateExport(T._("Tabellenfahrplan/Kursbuch als HTML Datei (*.html)|*.html"), GetTemplateChooser);
        var preview = new DefaultPreview("kfpl", T._("Kursbuch"), export);
        componentRegistry.Register<IExport>(export);
        componentRegistry.Register<IPreviewAction>(preview);
            
        componentRegistry.Register<IFilterRuleContainer>(FilterRuleContainer);
        componentRegistry.Register<IAppearanceControl>(new DefaultAppearanceControl(pi => new SettingsControl(pi), T._("Kursbuch")));
            
        componentRegistry.Register<ITimetableTypeChangeAction>(new FixAttrsAction());
            
        InitTemplates(pluginInterface, componentRegistry);
    }
        
    public void InitTemplates(IPluginInterface pluginInterface, IComponentRegistry componentRegistry)
    {
        componentRegistry.Register<ITemplateProvider>(new Templates.TemplateProvider());
            
        componentRegistry.Register<ITemplateWhitelistEntry>(new TemplateWhitelistEntry<Templates.TemplateHelper>("kfpl"));
        componentRegistry.Register<ITemplateWhitelistEntry>(new TemplateWhitelistEntry<KfplAttrs>("kfpl"));
    }

    internal static IFilterRuleContainer FilterRuleContainer => new DefaultFilterRuleContainer(T._("Kursbuch"), KfplAttrs.GetAttrs, KfplAttrs.CreateAttrs);
        
    internal static ITemplateChooser GetTemplateChooser(IReducedPluginInterface pi) 
        => new DefaultTemplateChooser(pi, "kfpl", "kfpl_attrs", "tmpl", "builtin:FPLedit.Kursbuch/Templates/KfplTemplate.fpltmpl");
}

public sealed class FixAttrsAction : BaseConverterFileType, ITimetableTypeChangeAction
{
    public void ToLinear(Timetable tt)
    {
        var attrs = KfplAttrs.GetAttrs(tt);
        if (attrs == null)
            return;
        var route = tt.GetRoutes().Single().Index;
        ConvertAttrNetToLin(attrs.KBSn, route);
    }

    public void ToNetwork(Timetable tt)
    {
        var attrs = KfplAttrs.GetAttrs(tt);
        if (attrs == null)
            return;
        ConvertAttrLinToNet(attrs.KBSn);
    }
}
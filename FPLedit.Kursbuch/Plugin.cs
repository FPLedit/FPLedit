using System.Collections.Generic;
using System.Linq;
using FPLedit.Kursbuch.Forms;
using FPLedit.Kursbuch.Model;
using FPLedit.Shared;
using FPLedit.Shared.DefaultImplementations;
using FPLedit.Shared.Filetypes;
using FPLedit.Shared.Templating;

namespace FPLedit.Kursbuch
{
    [Plugin("Modul für Tabellenfahrpläne", Vi.PFrom, Vi.PUpTo, Author = "Manuel Huber")]
    public sealed class Plugin : IPlugin
    {
        public void Init(IPluginInterface pluginInterface)
        {
            var export = new DefaultTemplateExport("Tabellenfahrplan/Kursbuch als HTML Datei (*.html)|*.html", GetTemplateChooser);
            var preview = new DefaultPreview("kfpl", "Kursbuch", export);
            pluginInterface.Register<IExport>(export);
            pluginInterface.Register<IPreviewAction>(preview);
            
            pluginInterface.Register<IFilterRuleContainer>(FilterRuleContainer);
            pluginInterface.Register<IAppearanceControl>(new DefaultAppearanceControl(pi => new SettingsControl(pi), "Kursbuch"));

            pluginInterface.Register<ITemplateProvider>(new Templates.TemplateProvider());
            
            pluginInterface.Register<ITemplateWhitelistEntry>(new TemplateWhitelistEntry<Templates.TemplateHelper>("kfpl"));
            pluginInterface.Register<ITemplateWhitelistEntry>(new TemplateWhitelistEntry<Model.KfplAttrs>("kfpl"));

            pluginInterface.Register<ITimetableTypeChangeAction>(new FixAttrsAction());
        }

        internal static IFilterRuleContainer FilterRuleContainer => new DefaultFilterRuleContainer("Kursbuch", KfplAttrs.GetAttrs, KfplAttrs.CreateAttrs);
        
        internal static ITemplateChooser GetTemplateChooser(IReducedPluginInterface pi) 
            => new DefaultTemplateChooser(pi, "kfpl", "kfpl_attrs", "tmpl", "builtin:FPLedit.Kursbuch/Templates/KfplTemplate.fpltmpl");
    }

    public sealed class FixAttrsAction : BaseConverterFileType, ITimetableTypeChangeAction
    {
        public void ToLinear(Timetable tt)
        {
            var attrs = Model.KfplAttrs.GetAttrs(tt);
            if (attrs == null)
                return;
            var route = tt.GetRoutes().FirstOrDefault().Index;
            ConvertAttrNetToLin(attrs.KBSn, route);
        }

        public void ToNetwork(Timetable tt)
        {
            var attrs = Model.KfplAttrs.GetAttrs(tt);
            if (attrs == null)
                return;
            ConvertAttrLinToNet(attrs.KBSn);
        }
    }
}

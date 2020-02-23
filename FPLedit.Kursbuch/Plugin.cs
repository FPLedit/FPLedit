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
            var export = new BasicTemplateExport("Tabellenfahrplan/Kursbuch als HTML Datei (*.html)|*.html", GetTemplateChooser);
            var preview = new BasicPreview("kfpl", "Kursbuch", export);
            pluginInterface.Register<IExport>(export);
            pluginInterface.Register<IPreviewProxy>(preview);
            
            pluginInterface.Register<IFilterableProvider>(FilterableProvider);
            pluginInterface.Register<IAppearanceControl>(new BasicAppearanceControl(pi => new SettingsControl(pi), "Kursbuch"));

            pluginInterface.Register<ITemplateProxy>(new Templates.TemplateProxy());
            
            pluginInterface.Register<ITemplateWhitelist>(new TemplateWhitelist<Templates.TemplateHelper>("kfpl"));
            pluginInterface.Register<ITemplateWhitelist>(new TemplateWhitelist<Model.KfplAttrs>("kfpl"));

            pluginInterface.Register<ITimetableTypeChangeAction>(new FixAttrsAction());
        }

        internal static IFilterableProvider FilterableProvider => new BasicFilterableProvider("Kursbuch", KfplAttrs.GetAttrs, KfplAttrs.CreateAttrs);
        
        internal static ITemplateChooser GetTemplateChooser(IReducedPluginInterface pi) 
            => new BasicTemplateChooser(pi, "kfpl", "kfpl_attrs", "tmpl", "builtin:FPLedit.Kursbuch/Templates/KfplTemplate.fpltmpl");
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

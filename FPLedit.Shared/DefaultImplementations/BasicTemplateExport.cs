using System;
using System.IO;
using System.Linq;
using FPLedit.Shared.Templating;

namespace FPLedit.Shared.DefaultImplementations
{
    public sealed class BasicTemplateExport : IExport
    {
        private readonly Func<IPluginInterface, BaseTemplateChooser> getChooser;
        public string Filter { get; }
        
        public BasicTemplateExport(string filter, Func<IPluginInterface, BaseTemplateChooser> getChooser)
        {
            this.getChooser = getChooser;
            Filter = filter;
        }

        public bool Export(Timetable tt, string filename, IPluginInterface pluginInterface, string[] flags = null)
        {
            var chooser = getChooser(pluginInterface);
            var templ = chooser.GetTemplate(tt);
            string cont = templ.GenerateResult(tt);

            if (cont == null)
                return false;

            if (flags?.Contains("tryout_console") ?? false)
                cont += ResourceHelper.GetStringResource("Shared.Resources.TryoutConsole.html");

            File.WriteAllText(filename, cont);

            return true;
        }
    }
}
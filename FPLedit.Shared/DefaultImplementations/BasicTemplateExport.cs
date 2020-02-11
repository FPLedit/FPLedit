using System;
using System.IO;
using System.Linq;
using FPLedit.Shared.Templating;

namespace FPLedit.Shared.DefaultImplementations
{
    public sealed class BasicTemplateExport : IExport
    {
        public const string FLAG_TYROUT_CONSOLE = "tryout_console";
        
        private readonly Func<IPluginInterface, BaseTemplateChooser> getChooser;
        public string Filter { get; }
        
        public BasicTemplateExport(string filter, Func<IPluginInterface, BaseTemplateChooser> getChooser)
        {
            this.getChooser = getChooser;
            Filter = filter;
        }

        public bool Export(Timetable tt, Stream stream, IPluginInterface pluginInterface, string[] flags = null)
        {
            var chooser = getChooser(pluginInterface);
            var templ = chooser.GetTemplate(tt);
            string cont = templ.GenerateResult(tt);

            if (cont == null)
                return false;

            if (flags?.Contains(FLAG_TYROUT_CONSOLE) ?? false)
                cont += ResourceHelper.GetStringResource("Shared.Resources.TryoutConsole.html");

            using (var sw = new StreamWriter(stream))
                sw.Write(cont);

            return true;
        }
    }
}
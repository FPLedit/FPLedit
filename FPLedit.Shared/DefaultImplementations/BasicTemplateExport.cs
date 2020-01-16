using System.IO;
using System.Linq;
using FPLedit.Shared.Templating;

namespace FPLedit.Shared.DefaultImplementations
{
    public sealed class BasicTemplateExport : IExport
    {
        private readonly BaseTemplateChooser chooser;
        
        public string Filter { get; }
        
        public BasicTemplateExport(string filter, BaseTemplateChooser chooser)
        {
            Filter = filter;
            this.chooser = chooser;
        }

        public bool Export(Timetable tt, string filename, IPluginInterface pluginInterface, string[] flags = null)
        {
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
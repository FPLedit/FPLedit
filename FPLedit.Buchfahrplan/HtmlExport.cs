using FPLedit.Buchfahrplan.Model;
using FPLedit.Shared;
using FPLedit.Shared.Templating;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace FPLedit.Buchfahrplan
{
    public class HtmlExport : IExport
    {
        public string Filter => "Buchfahrplan als HTML Datei (*.html)|*.html";

        internal bool Exp(Timetable timetable, string filename, IPluginInterface pluginInterface, bool tryout_console)
        {
            BfplTemplateChooser chooser = new BfplTemplateChooser(pluginInterface);

            ITemplate tmpl = chooser.GetTemplate(timetable);
            string cont = tmpl.GenerateResult(timetable);

            if (cont == null)
                return false;

            if (tryout_console)
                cont += global::ResourceHelper.GetStringResource("Buchfahrplan.Resources.TryoutScript.txt");

            File.WriteAllText(filename, cont);

            return true;
        }

        public bool Export(Timetable timetable, string filename, IPluginInterface pluginInterface)
            => Exp(timetable, filename, pluginInterface, false);
    }
}

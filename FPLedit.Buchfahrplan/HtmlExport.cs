using FPLedit.Buchfahrplan.Model;
using FPLedit.Buchfahrplan.Properties;
using FPLedit.Shared;
using FPLedit.Shared.Templating;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace FPLedit.Buchfahrplan
{
    public class HtmlExport : IExport
    {
        public string Filter => "Buchfahrplan als HTML Datei (*.html)|*.html";

        private bool Exp(Timetable timetable, string filename, IInfo info, bool tryout_console)
        {
            BfplTemplateChooser chooser = new BfplTemplateChooser(info);

            ITemplate tmpl = chooser.GetTemplate(timetable);
            string cont = tmpl.GenerateResult(timetable);

            if (tryout_console)
                cont += Resources.TryoutScript;

            File.WriteAllText(filename, cont);

            return true;
        }

        public bool Export(Timetable timetable, string filename, IInfo info)
            => Exp(timetable, filename, info, false);

        public bool ExportTryoutConsole(Timetable timetable, string filename, IInfo info)
            => Exp(timetable, filename, info, true);
    }
}

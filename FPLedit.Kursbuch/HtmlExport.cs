using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FPLedit.Kursbuch
{
    public class HtmlExport : IExport
    {
        public string Filter => "Aushangfahrplan als HTML Datei (*.html)|*.html";

        private bool Exp(Timetable tt, string filename, IInfo info, bool tryout_console)
        {
            var chooser = new AfplTemplateChooser(info);

            IKfplTemplate templ = chooser.GetTemplate(tt);
            string cont = templ.GetResult(tt);

            //if (tryout_console)
            //    cont += Resources.TryoutScript;

            File.WriteAllText(filename, cont);

            return true;
        }

        public bool Export(Timetable tt, string filename, IInfo info)
            => Exp(tt, filename, info, false);

        public bool ExportTryoutConsole(Timetable tt, string filename, IInfo info)
            => Exp(tt, filename, info, true);
    }
}

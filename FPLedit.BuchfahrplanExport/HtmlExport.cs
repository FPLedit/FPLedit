using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace FPLedit.BuchfahrplanExport
{
    public class HtmlExport : IExport
    {

        public string Filter
        {
            get { return "Buchfahrplan als HTML Datei (*.html)|*.html"; }
        }

        public bool Export(Timetable timetable, string filename, ILog logger)
        {
            BuchfahrplanTemplate templ = new BuchfahrplanTemplate(timetable, false);
            string cont = templ.TransformText();
            File.WriteAllText(filename, cont);
            return true;
        }

        public bool ExportTryoutConsole(Timetable timetable, string filename, ILog logger)
        {
            BuchfahrplanTemplate templ = new BuchfahrplanTemplate(timetable, true);
            string cont = templ.TransformText();
            File.WriteAllText(filename, cont);
            return true;
        }
    }
}

using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace FPLedit.AushangfahrplanExport
{
    public class HtmlExport : IExport
    {
        public string Filter => "Aushangfahrplan als HTML Datei (*.html)|*.html";

        public bool Export(Timetable tt, string filename, IInfo info)
        {
            AfplTemplate templ = new AfplTemplate(tt);
            string cont = templ.TransformText();
            File.WriteAllText(filename, cont);

            return true;
        }
    }
}

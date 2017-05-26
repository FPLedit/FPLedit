﻿using FPLedit.BuchfahrplanExport.Model;
using FPLedit.BuchfahrplanExport.Properties;
using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace FPLedit.BuchfahrplanExport
{
    public class HtmlExport : IExport
    {
        public string Filter
        {
            get { return "Buchfahrplan als HTML Datei (*.html)|*.html"; }
        }

        private bool Exp(Timetable timetable, string filename, IInfo info, bool tryout_console)
        {
            IBfplTemplate tmpl = new BuchfahrplanTemplate();
            string cont = tmpl.GetTranformedText(timetable);

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

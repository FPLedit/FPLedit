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

        public bool Export(Timetable timetable, string filename, IInfo info)
        {
            var attrsEn = timetable.Children.FirstOrDefault(x => x.XName == "bfpl_attrs");

            if (attrsEn != null)
            {
                var attrs = new BFPL_Attrs(attrsEn, timetable);
                var css = attrs.Css ?? "";

                var pattern = "@import\\s+(url\\()?['\"]([\\w\\-. ]+)['\"](\\))?[\\w, ]*;";
                MatchCollection matches = Regex.Matches(css, pattern);

                var srcDir = Path.GetDirectoryName(info.FileState.FileName);
                css = Regex.Replace(css, pattern, m =>
                {
                    var fn = m.Groups[2].Value;
                    var src = Path.Combine(srcDir, fn);
                    if (!File.Exists(src))
                        return "";
                    return File.ReadAllText(src);
                });


                //var fns = matches.OfType<Match>().Select(m => m.Groups[2].Value).Distinct();

                //var destDir = Path.GetDirectoryName(filename);
                //var srcDir = Path.GetDirectoryName(info.FileState.FileName);
                //foreach (var fn in fns)
                //{
                //    var dest = Path.Combine(destDir, fn);
                //    var src = Path.Combine(srcDir, fn);
                //    if (!File.Exists(src))
                //        continue;
                //    File.Copy(src, dest);
                //}
            }


            BuchfahrplanTemplate templ = new BuchfahrplanTemplate(timetable, false);
            string cont = templ.TransformText();
            File.WriteAllText(filename, cont);
            return true;
        }

        public bool ExportTryoutConsole(Timetable timetable, string filename, IInfo info)
        {
            BuchfahrplanTemplate templ = new BuchfahrplanTemplate(timetable, true);
            string cont = templ.TransformText();
            File.WriteAllText(filename, cont);
            return true;
        }
    }
}

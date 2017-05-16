using FPLedit.BuchfahrplanExport.Model;
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
            bool enable_atimports = bool.Parse(SettingsManager.Get("bfpl.beta_atimports", "false"));

            if (enable_atimports)
                IncludeImports(timetable, info);

            IBfplTemplate tmpl = new BuchfahrplanTemplate(false);
            string cont = tmpl.GetTranformedText(timetable);
            File.WriteAllText(filename, cont);

            if (enable_atimports)
                RecoverCss(timetable);

            return true;
        }

        public bool ExportTryoutConsole(Timetable timetable, string filename, IInfo info)
        {
            bool enable_atimports = bool.Parse(SettingsManager.Get("bfpl.beta_atimports", "false"));

            if (enable_atimports)
                IncludeImports(timetable, info);

            IBfplTemplate tmpl = new BuchfahrplanTemplate(true);
            string cont = tmpl.GetTranformedText(timetable);
            File.WriteAllText(filename, cont);

            if (enable_atimports)
                RecoverCss(timetable);

            return true;
        }

        private bool recover_css = false;
        private string old_css;

        private void RecoverCss(Timetable tt)
        {
            if (!recover_css)
                return;

            var attrsEn = tt.Children.FirstOrDefault(x => x.XName == "bfpl_attrs");

            if (attrsEn != null)
            {
                var attrs = new BFPL_Attrs(attrsEn, tt);
                attrs.Css = old_css;
            }

            old_css = null;
            recover_css = false;
        }

        private void IncludeImports(Timetable tt, IInfo info)
        {
            var attrsEn = tt.Children.FirstOrDefault(x => x.XName == "bfpl_attrs");

            if (attrsEn != null)
            {
                var attrs = new BFPL_Attrs(attrsEn, tt);
                old_css = attrs.Css ?? "";

                var pattern = "@import\\s+(url\\()?['\"]([\\w\\-.\\/\\\\: ]+)['\"](\\))?[\\w, ]*;";
                MatchCollection matches = Regex.Matches(old_css, pattern);

                var srcDir = Path.GetDirectoryName(info.FileState.FileName);
                List<string> srcs = new List<string>();
                var new_css = Regex.Replace(old_css, pattern, m =>
                {
                    var fn = m.Groups[2].Value;
                    var src = Path.GetFullPath(Path.Combine(srcDir, fn));
                    if (!File.Exists(src) || srcs.Contains(src))
                        return "";
                    srcs.Add(src);
                    return File.ReadAllText(src);
                });

                attrs.Css = new_css.Trim();
                recover_css = true;
            }
        }
    }
}

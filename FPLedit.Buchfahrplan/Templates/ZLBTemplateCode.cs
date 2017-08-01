using FPLedit.Buchfahrplan.Model;
using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FPLedit.Buchfahrplan.Templates
{
    partial class ZLBTemplate : IBfplTemplate
    {
        private Timetable tt;
        private string font = "serif";
        private string additionalCss = "";
        private BfplAttrs attrs;

        public string Name => "Vorlage für den Zugleitbetrieb";

        private readonly TemplateHelper helper = new TemplateHelper();

        public string GetResult(Timetable tt)
        {
            this.tt = tt;
            helper.TT = tt;

            attrs = BfplAttrs.GetAttrs(tt);
            if (attrs != null)
            {
                if (attrs.Font != "")
                    font = attrs.Font;
                additionalCss = attrs.Css ?? "";
                helper.Attrs = attrs;
            }

            GenerationEnvironment = null; //BUGFIX: Clear last build
            return TransformText();
        }

        private string GetWelle(ref int counter, IStation sta, Train tra)
        {
            if (counter > 0)
            {
                counter--;
                return "";
            }
            else if (sta.Wellenlinien != 0)
            {
                var w = sta.Wellenlinien;
                var stas = helper.GetStations(tra.Direction);
                stas = stas.Skip(stas.IndexOf(sta)).ToList();
                counter = stas.TakeWhile(s => s.Wellenlinien == w).Count();
                return $"<td class=\"zug welle welle{w}\" rowspan=\"{counter--}\"></td>";
            }

            return "<td class=\"zug welle\"></td>";
        }
    }
}

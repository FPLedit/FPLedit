using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FPLedit.BuchfahrplanExport
{
    partial class BuchfahrplanTemplate
    {
        private Timetable tt;
        private string font;
        private string additionalCss;

        public BuchfahrplanTemplate(Timetable tt)
        {
            this.tt = tt;
            font = tt.GetAttribute("bFont", "\"Alte DIN 1451 Mittelschrift\"");
            //additionalCss = tt.GetMeta("BuchfahrplanCSS", ""); //TODO: auf Dateinamen umstellen!
            additionalCss = "";
        }

        private string HtmlId(string text)
        {
            return "train-" + text.Replace("#", "")
                .Replace(" ", "-")
                .Replace(".", "-")
                .Replace(":", "-")
                .ToLower();
        }
    }
}

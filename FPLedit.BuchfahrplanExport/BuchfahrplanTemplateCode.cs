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
        private string font = "\"Alte DIN 1451 Mittelschrift\"";
        private string additionalCss = "";

        public BuchfahrplanTemplate(Timetable tt)
        {
            this.tt = tt;
            var dataEn = tt.Children.FirstOrDefault(x => x.XName == "bfpl_attrs");

            if (dataEn != null)
            {
                var data = new BFPL_Attrs(dataEn, tt);
                font = data.Font;
                additionalCss = data.Css ?? "";
            }
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

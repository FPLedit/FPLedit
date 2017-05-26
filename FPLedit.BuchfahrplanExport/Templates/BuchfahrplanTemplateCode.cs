using FPLedit.BuchfahrplanExport.Model;
using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FPLedit.BuchfahrplanExport.Templates
{
    partial class BuchfahrplanTemplate : IBfplTemplate
    {
        private Timetable tt;
        private string font = "\"Alte DIN 1451 Mittelschrift\"";
        private string additionalCss = "";
        private BfplAttrs attrs;

        public string Name => "Standard-Buchfahrplan";

        private readonly TemplateHelper helper = new TemplateHelper();

        public string GetTranformedText(Timetable tt)
        {
            this.tt = tt;
            helper.TT = tt;
            var attrsEn = tt.Children.FirstOrDefault(x => x.XName == "bfpl_attrs");

            if (attrsEn != null)
            {
                attrs = new BfplAttrs(attrsEn, tt);
                if (attrs.Font != "")
                    font = attrs.Font;
                additionalCss = attrs.Css ?? "";
                helper.Attrs = attrs;
            }

            return TransformText();
        }
    }
}

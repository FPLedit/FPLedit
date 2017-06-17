using FPLedit.BuchfahrplanExport.Model;
using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FPLedit.BuchfahrplanExport.Templates
{
    partial class ZLBTemplate : IBfplTemplate
    {
        private Timetable tt;
        private string font = "serif";
        private string additionalCss = "";
        private BfplAttrs attrs;

        public string Name => "Vorlage für den Zugleitbetrieb";

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

        private string OptAttr(string caption, string value)
        {
            if (value != null && value != "")
                return caption + " " + value;
            return "";
        }
    }
}

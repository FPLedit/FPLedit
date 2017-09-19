using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FPLedit.Shared;
using FPLedit.Kursbuch.Model;

namespace FPLedit.Kursbuch.Templates
{
    partial class KfplTemplate : IKfplTemplate
    {
        private Timetable tt;
        private string kbs = "";
        private string font = "Arial"; //TODO: KBZiffern
        private string heFont = "\"Arial Black\"";
        private string additionalCss = "";

        private const TrainDirection TOP_DIRECTION = TrainDirection.ti;
        private const TrainDirection BOTTOM_DIRECTION = TrainDirection.ta;

        public string Name => "Standardvorlage (DB-Kursbuch)";

        private TemplateHelper helper;

        public string GetResult(Timetable tt)
        {
            this.tt = tt;
            helper = new TemplateHelper(tt);

            var attrs = KfplAttrs.GetAttrs(tt);
            if (attrs != null)
            {
                if (attrs.Font != "")
                    font = attrs.Font;
                if (attrs.HeFont != "")
                    heFont = attrs.HeFont;
                additionalCss = attrs.Css ?? "";
                if (attrs.Kbs != "")
                    kbs = attrs.Kbs;
            }

            GenerationEnvironment = null; //BUGFIX: Clear last build
            return TransformText();
        }

        private string Sign(Train t)
        {
            return "";
        }
    }
}

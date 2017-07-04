using FPLedit.AushangfahrplanExport.Model;
using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FPLedit.AushangfahrplanExport.Templates
{
    partial class AfplTemplate : IAfplTemplate
    {
        private Timetable tt;
        private string font = "Arial";
        private string additionalCss = "";

        public string Name => "Standard (DRG aus Malsch)";

        private TemplateHelper helper;

        public string GetTranformedText(Timetable tt)
        {
            this.tt = tt;
            helper = new TemplateHelper(tt);

            var attrs = AfplAttrs.GetAttrs(tt);
            if (attrs != null)
            {
                if (attrs.Font != "")
                    font = attrs.Font;
                additionalCss = attrs.Css ?? "";
            }

            return TransformText();
        }

        private string GetTimeString(TimeSpan t)
            => t.Hours.ToString() + "<sup>" + t.Minutes.ToString("00") + "</sup>";

        private string TimeString(Train[] trains, Station sta, int i)
            => trains.Count() > i ? GetTimeString(trains[i].GetArrDep(sta).Departure) : "";

        private string NameString(Train[] trains, int i)
            => trains.Count() > i ? trains[i].TName : "";
    }
}

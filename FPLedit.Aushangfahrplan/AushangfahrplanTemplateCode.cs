using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FPLedit.AushangfahrplanExport
{
    partial class AushangfahrplanTemplate
    {
        private Timetable tt;

        public AushangfahrplanTemplate(Timetable tt)
        {
            this.tt = tt;
        }

        public string GetTimeString(TimeSpan t)
        {
            return t.Hours.ToString()+"<sup>" + t.Minutes.ToString("00")+"</sup>";
        }
    }
}

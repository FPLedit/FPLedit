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

        public BuchfahrplanTemplate(Timetable tt)
        {
            this.tt = tt;
            font = tt.GetMeta("BuchfahrplanFont", "\"Alte DIN 1451 Mittelschrift\"");
        }
    }
}

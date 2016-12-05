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

        public const bool TOP_DIRECTION = false;
        public const bool BOTTOM_DIRECTION = true;

        public BuchfahrplanTemplate(Timetable tt)
        {
            this.tt = tt;
        }
    }
}

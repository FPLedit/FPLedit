using FPLedit.Shared;
using FPLedit.Shared.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FPLedit.TimetableChecks
{
    public class UpdateColorsAction : ITimetableInitAction
    {
        public void Init(Timetable tt)
        {
            ColorTimetableConverter.ConvertAll(tt);
        }
    }
}

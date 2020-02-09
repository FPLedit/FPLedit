using FPLedit.Shared;
using FPLedit.Shared.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FPLedit.TimetableChecks
{
    internal sealed class UpdateColorsAction : ITimetableInitAction
    {
        public string Init(Timetable tt)
        {
            // Farbangaben zwischen jTG- und FPLedit-Versionen vereinheitlichen
            ColorTimetableConverter.ConvertAll(tt);
            return null;
        }
    }
}

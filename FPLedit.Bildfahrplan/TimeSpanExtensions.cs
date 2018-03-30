using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace FPLedit.BildfahrplanExport
{
    public static class TimeSpanExtensions
    {
        [DebuggerStepThrough]
        public static int GetHours(this TimeSpan span)
            => ((span.Days * 24) + span.Hours);

        [DebuggerStepThrough]
        public static int GetMinutes(this TimeSpan span)
            => ((span.Days * 24) + span.Hours) * 60 + span.Minutes;

        [DebuggerStepThrough]
        public static int GetSeconds(this TimeSpan span)
            => (((span.Days * 24) + span.Hours) * 60 + span.Minutes) * 60 + span.Seconds;

        public static string ToString(this Station sta, bool kilometre, int route)
            => sta.SName + (kilometre ? (" (" + sta.Positions.GetPosition(route) + ")") : "");
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace FPLedit.Shared
{
    public static class TimeSpanExtensions
    {
        [DebuggerStepThrough]
        public static string ToShortTimeString(this TimeSpan span)
        {
            return span.ToString(@"hh\:mm");
        }

        [DebuggerStepThrough]
        public static int GetHours(this TimeSpan span)
        {
            return ((span.Days * 24) + span.Hours);
        }

        [DebuggerStepThrough]
        public static int GetMinutes(this TimeSpan span)
        {
            return ((span.Days * 24) + span.Hours) * 60 + span.Minutes;
        }

        [DebuggerStepThrough]
        public static int GetSeconds(this TimeSpan span)
        {
            return (((span.Days * 24) + span.Hours) * 60 + span.Minutes) * 60 + span.Seconds;
        }
    }
}

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
            if (span.Days > 0)
                return "24:00"; //TODO: Testen
            return span.ToString(@"hh\:mm");
        }
    }
}

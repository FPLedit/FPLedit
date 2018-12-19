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
            // Das ist ein Hack. Die Methode sollte nur mit "Zeitangaben" verwendet werden.
            if (span.Days > 0)
                return "24:00";
            return span.ToString(@"hh\:mm");
        }
    }
}

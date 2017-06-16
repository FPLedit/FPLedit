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
            => span.ToString(@"hh\:mm");
    }
}

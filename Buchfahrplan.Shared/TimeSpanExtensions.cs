using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Buchfahrplan.Shared
{
    public static class TimeSpanExtensions
    {
        public static string ToShortTimeString(this TimeSpan span)
        {
            return span.ToString(@"hh\:mm");
        }

        public static int GetHours(this TimeSpan span)
        {
            return ((span.Days * 24) + span.Hours);
        }

        public static int GetMinutes(this TimeSpan span)
        {
            return ((span.Days * 24) + span.Hours) * 60 + span.Minutes;
        }

        public static int GetSeconds(this TimeSpan span)
        {
            return (((span.Days * 24) + span.Hours) * 60 + span.Minutes) * 60 + span.Seconds;
        }
    }
}

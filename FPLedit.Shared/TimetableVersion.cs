using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FPLedit.Shared
{
    public enum TimetableVersion : int
    {
        JTG2_x = 008,
        JTG3_0 = 009,
        JTG3_1 = 010,
        Extended_FPL = 100,
    }

    public static class TimetableVersionExt
    {
        public static string ToNumberString(this TimetableVersion version)
            => ((int)version).ToString("000");
    }
}

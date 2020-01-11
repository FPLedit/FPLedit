using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FPLedit.Shared
{
    [TemplateSafe]
    public enum TimetableVersion : int
    {
        JTG2_x = 008,
        JTG3_0 = 009,
        JTG3_1 = 010,
        Extended_FPL = 100,
    }

    [TemplateSafe]
    public static class TimetableVersionExt
    {
        public static string ToNumberString(this TimetableVersion version)
            => ((int)version).ToString("000");

        /// <summary>
        /// Vergleicht zwei TimetableVersions.
        /// </summary>
        /// <returns>
		/// Kleiner als 0: Diese Instanz ist kleiner als <paramref name="value" />.
		/// 0: Diese Instanz ist gleich <paramref name="value" />.
		/// Größer als 0: Diese Instanz ist größer als <paramref name="value" />.
        /// </returns>
        public static int Compare(this TimetableVersion version, TimetableVersion value)
            => ((int)version).CompareTo((int)value);
    }
}

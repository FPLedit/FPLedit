namespace FPLedit.Shared
{
    /// <summary>
    /// File version of the timetable format.
    /// </summary>
    /// <remarks><see cref="ITimetable.Version"/> may have other values from those defined in the enum, but those are not supported.</remarks>
    [Templating.TemplateSafe]
    public enum TimetableVersion : int
    {
        JTG2_x = 008,
        JTG3_0 = 009,
        JTG3_1 = 010,
        JTG3_2 = 011,
        Extended_FPL = 100,
    }

    [Templating.TemplateSafe]
    public static class TimetableVersionExt
    {
        /// <summary>
        /// Returns a three digit decimal string representation of theis Timetable Version.
        /// </summary>
        public static string ToNumberString(this TimetableVersion version)
            => ((int)version).ToString("000");

        /// <summary>
        /// Compares two timetable versions.
        /// </summary>
        /// <returns>
        /// <list type="bullet">
		/// <item><description>Less than zero: This instance is smaller than <paramref name="value" />.</description></item>
		/// <item><description>Zero: This instance equals <paramref name="value" />.</description></item>
		/// <item><description>Greater than zero: Diese Instanz ist größer als <paramref name="value" />.</description></item>
		/// </list>
        /// </returns>
        public static int Compare(this TimetableVersion version, TimetableVersion value)
            => ((int)version).CompareTo((int)value);
    }
}

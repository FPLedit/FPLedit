using System;
using System.Linq;

namespace FPLedit.Shared
{
    /// <summary>
    /// File version of the timetable format.
    /// </summary>
    /// <remarks><see cref="ITimetable.Version"/> may have other values from those defined in the enum, but those are not supported.</remarks>
    [Templating.TemplateSafe]
    public enum TimetableVersion
    {
        // ReSharper disable InconsistentNaming
        [TtVersionCompat(TtVersionCompatType.UpgradeOnly)]
        [JtgVersionCompat("2.02", TtVersionJtgCompat.OnlyLinear)]
        [JtgVersionCompat("2.03", TtVersionJtgCompat.OnlyLinear)]
        JTG2_x = 008,

        [TtVersionCompat(TtVersionCompatType.UpgradeOnly)]
        [JtgVersionCompat("3.03", TtVersionJtgCompat.OnlyLinear)]
        JTG3_0 = 009,

        [TtVersionCompat(TtVersionCompatType.ReadWrite)]
        [JtgVersionCompat("3.11", TtVersionJtgCompat.OnlyLinear)]
        JTG3_1 = 010,
        
        [TtVersionCompat(TtVersionCompatType.ReadWrite)]
        [JtgVersionCompat("3.2*", TtVersionJtgCompat.OnlyLinear)]
        JTG3_2 = 011,

        [TtVersionCompat(TtVersionCompatType.ReadWrite)]
        Extended_FPL = 100,
        // ReSharper restore InconsistentNaming
    }

    [Templating.TemplateSafe]
    public static class TimetableVersionExt
    {
        /// <summary>
        /// Returns a three digit decimal string representation of theis Timetable Version.
        /// </summary>
        public static string ToNumberString(this TimetableVersion version)
            => ((int) version).ToString("000");

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
            => ((int) version).CompareTo((int) value);

        public static TtVersionCompatType GetCompat(this TimetableVersion version) => GetAttribute(version)?.CompatType ?? TtVersionCompatType.None;
        
        //public static TtVersionJtgCompat? GetJtgCompat(this TimetableVersion version) => GetAttribute(version)?.JtgCompat;

        private static TtVersionCompatAttribute? GetAttribute(TimetableVersion version)
        {
            var type = typeof(TimetableVersion);
            var memInfo = type.GetMember(version.ToString());
            var mem = memInfo.FirstOrDefault(m => m.DeclaringType == type);
            var attributes = mem?.GetCustomAttributes(typeof(TtVersionCompatAttribute), false);
            return (TtVersionCompatAttribute?) attributes?.FirstOrDefault();
        }
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class TtVersionCompatAttribute : Attribute
    {
        public TtVersionCompatType CompatType { get; }

        public TtVersionCompatAttribute(TtVersionCompatType compatType)
        {
            CompatType = compatType;
        }
    }
    
    
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class JtgVersionCompatAttribute : Attribute
    {
        public TtVersionJtgCompat CompatType { get; }

        public string Version { get; }

        public const string ALL_VERSIONS = "ALL";

        public JtgVersionCompatAttribute(string version, TtVersionJtgCompat compatType)
        {
            Version = version;
            CompatType = compatType;
        }
    }

    public enum TtVersionCompatType
    {
        None,
        UpgradeOnly,
        ReadWrite,
    }

    public enum TtVersionJtgCompat
    {
        OnlyLinear,
        FullWithNetwork, // Currently unused (for future versions of jTrainGraph that support network timetables)
    }
}
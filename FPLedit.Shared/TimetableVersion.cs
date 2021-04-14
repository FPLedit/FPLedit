using System;
using System.Collections.Generic;
using System.Linq;

namespace FPLedit.Shared
{
    /// <summary>
    /// File version of the timetable format.
    /// </summary>
    /// <remarks><see cref="ITimetable.Version"/> may have other values from those defined in the enum, but those are not supported by FPLedit.</remarks>
    [Templating.TemplateSafe]
    public enum TimetableVersion
    {
        // ReSharper disable InconsistentNaming
        [TtVersionCompat(TtVersionCompatType.UpgradeOnly, TimetableType.Linear)]
        [JtgVersionCompat("2.02", TtVersionJtgCompat.OnlyLinear)]
        [JtgVersionCompat("2.03", TtVersionJtgCompat.OnlyLinear)]
        JTG2_x = 008,

        [TtVersionCompat(TtVersionCompatType.UpgradeOnly, TimetableType.Linear)]
        [JtgVersionCompat("3.03", TtVersionJtgCompat.OnlyLinear)]
        JTG3_0 = 009,

        [TtVersionCompat(TtVersionCompatType.UpgradeOnly, TimetableType.Linear)]
        [JtgVersionCompat("3.11", TtVersionJtgCompat.OnlyLinear)]
        JTG3_1 = 010,
        
        [TtVersionCompat(TtVersionCompatType.UpgradeOnly, TimetableType.Linear)]
        [JtgVersionCompat("3.2*", TtVersionJtgCompat.OnlyLinear)]
        JTG3_2 = 011,
        
        [TtVersionCompat(TtVersionCompatType.ReadWrite, TimetableType.Linear)]
        [JtgVersionCompat("3.3*", TtVersionJtgCompat.OnlyLinear)]
        [JtgVersionCompat("3.4*", TtVersionJtgCompat.OnlyLinear)]
        JTG3_3 = 012,

        [TtVersionCompat(TtVersionCompatType.UpgradeOnly, TimetableType.Network)]
        Extended_FPL = 100,
        
        [TtVersionCompat(TtVersionCompatType.ReadWrite, TimetableType.Network)]
        Extended_FPL2 = 101,
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

        /// <summary>
        /// Compatibility map to cache compatibility data retrieved from attributes.
        /// </summary>
        private static readonly Dictionary<TimetableVersion, TimetableVersionCompat> compatTable = new Dictionary<TimetableVersion, TimetableVersionCompat>();
        
        /// <summary>
        /// Get compatibilzty information for this version.
        /// </summary>
        public static TimetableVersionCompat GetVersionCompat(this TimetableVersion version)
        {
            if (compatTable.TryGetValue(version, out var c))
                return c;
            
            var attr = GetAttributes<TtVersionCompatAttribute>(version)?.FirstOrDefault();
            var compatType = attr?.CompatType ?? TtVersionCompatType.None;
            var type = attr?.FileType ?? TimetableType.Linear;

            var jtgCompat = GetAttributes<JtgVersionCompatAttribute>(version).Select(j => (j.Version, j.CompatType)).ToArray();
            
            return (compatTable[version] = new TimetableVersionCompat(version, compatType, type, jtgCompat));
        }

        /// <summary>
        /// Get the compatibility information for versions defined in <see cref="TimetableVersion"/>.
        /// </summary>
        public static IEnumerable<TimetableVersionCompat> GetAllVersionInfos() 
            => Enum.GetValues(typeof(TimetableVersion)).Cast<TimetableVersion>().Select(v => v.GetVersionCompat());

        private static IEnumerable<T> GetAttributes<T>(TimetableVersion version)
        {
            var type = typeof(TimetableVersion);
            var memInfo = type.GetMember(version.ToString());
            var mem = memInfo.FirstOrDefault(m => m.DeclaringType == type);
            return mem?.GetCustomAttributes(typeof(T), false)?.Cast<T>() ?? Enumerable.Empty<T>();
        }
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    internal sealed class TtVersionCompatAttribute : Attribute
    {
        public TtVersionCompatType CompatType { get; }
        public TimetableType FileType { get; }

        public TtVersionCompatAttribute(TtVersionCompatType compatType, TimetableType fileType)
        {
            CompatType = compatType;
            FileType = fileType;
        }
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    internal sealed class JtgVersionCompatAttribute : Attribute
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
}
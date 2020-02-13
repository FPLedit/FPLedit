using System;
using FPLedit.Shared.Helpers;

namespace FPLedit.Shared
{
    [Templating.TemplateSafe]
    public readonly struct TimeEntry : IComparable, IComparable<TimeEntry>, IEquatable<TimeEntry>
    {
        public static readonly TimeEntry Zero = new TimeEntry(0,0);

        private static readonly TimeNormalizer normalizer = new TimeNormalizer();
        
        public short Minutes { get; }
        public short Hours { get; }

        public TimeEntry(int hours, int minutes)
        {
            Minutes = (short)(minutes % 60);
            Hours = (short)(hours + (minutes - minutes % 60) / 60);
            if (Minutes < 0 && Hours > 0)
            {
                Hours--;
                Minutes = (short)(60 + Minutes); //Minutes is negative
            }
        }

        public TimeEntry Add(TimeEntry entry) => new TimeEntry(Hours + entry.Hours, Minutes + entry.Minutes);
        public TimeEntry Substract(TimeEntry entry) => new TimeEntry(Hours - entry.Hours, Minutes - entry.Minutes);

        public int GetTotalMinutes() => Hours * 60 + Minutes;

        public int CompareTo(TimeEntry other)
        {
            if (other.Hours == Hours)
                return Minutes.CompareTo(other.Minutes);
            return Hours.CompareTo(other.Hours);
        }

        public override bool Equals(object obj)
        {
            if (obj is TimeEntry t)
                return Equals(t);
            return false;
        }

        public override int GetHashCode() => Hours << 16 | Minutes;

        public bool Equals(TimeEntry obj) => obj.Hours == Hours && obj.Minutes == Minutes;

        public int CompareTo(object obj)
        {
            switch (obj)
            {
                case null:
                    return 1;
                case TimeEntry t:
                    return CompareTo(t);
                default:
                    throw new ArgumentException(nameof(obj) + " is not a TimeEntry instance!");
            }
        }

        public override string ToString() => $"{Hours:00}:{Minutes:00}";

        public string ToShortTimeString() => ToString();

        public static bool operator ==(TimeEntry t1, TimeEntry t2) => t1.Equals(t2);
        public static bool operator !=(TimeEntry t1, TimeEntry t2) => !t1.Equals(t2);
        public static bool operator <(TimeEntry t1, TimeEntry t2) => t1.CompareTo(t2) < 0;
        public static bool operator >(TimeEntry t1, TimeEntry t2) => t1.CompareTo(t2) > 0;
        public static bool operator <=(TimeEntry t1, TimeEntry t2) => t1.CompareTo(t2) <= 0;
        public static bool operator >=(TimeEntry t1, TimeEntry t2) => t1.CompareTo(t2) >= 0;
        public static TimeEntry operator +(TimeEntry t1, TimeEntry t2) => t1.Add(t2);
        public static TimeEntry operator -(TimeEntry t1, TimeEntry t2) => t1.Substract(t2);
        public static TimeEntry operator -(TimeEntry t1) => Zero.Substract(t1);
        public static TimeEntry operator +(TimeEntry t1) => Zero.Add(t1);
        public static explicit operator TimeEntry(TimeSpan ts) => new TimeEntry(ts.Days * 24 + ts.Hours, ts.Minutes);

        public static bool TryParse(string s, out TimeEntry entry)
        {
            entry = new TimeEntry();
            if (normalizer.Normalize(s) == null)
                return false;
            var ret = TimeSpan.TryParse(s.Replace("24:", "1.00:"), out var ts);
            entry = new TimeEntry(ts.Days * 24 + ts.Hours, ts.Minutes);
            return ret;
        }
        
        public static TimeEntry Parse(string s)
        {
            if (normalizer.Normalize(s) == null)
                throw new FormatException("TimeEntry Format not recognized");
            var te =  (TimeEntry)TimeSpan.Parse(s.Replace("24:", "1.00:"));
            return te;
        }
    }
}

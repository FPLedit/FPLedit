using System;

namespace FPLedit.Shared
{
    /// <summary>
    /// Represents a single read-only time value, counting hours and minutes.
    /// </summary>
    //TODO: Throw if seconds are added if not supported (move creation logic to TimeEntryFactory)
    [Templating.TemplateSafe]
    public readonly struct TimeEntry : IComparable, IComparable<TimeEntry>, IEquatable<TimeEntry>
    {
        public static readonly TimeEntry Zero = new TimeEntry(0,0, 0);

        public short Minutes { get; }
        public short Hours { get; }
        public short Seconds { get; }

        public TimeEntry(int hours, int minutes, int seconds)
        {
            Seconds = (short)(seconds % 60);
            var m = minutes + (seconds - seconds % 60) / 60;
            Minutes = (short)(m % 60);
            Hours = (short)(hours + (m - m % 60) / 60);
            if (Minutes < 0 && Hours > 0)
            {
                Hours--;
                Minutes = (short)(60 + Minutes); // Minutes are negative
            }
        }

        public TimeEntry(int hours, int minutes) : this(hours, minutes, 0)
        {
        }

        public TimeEntry Add(TimeEntry entry) => new TimeEntry(Hours + entry.Hours, Minutes + entry.Minutes);
        public TimeEntry Substract(TimeEntry entry) => new TimeEntry(Hours - entry.Hours, Minutes - entry.Minutes);

        public int GetTotalMinutes() => Hours * 60 + Minutes;
        
        public TimeEntry Normalize() => new TimeEntry(Hours % 24, Minutes);

        public int CompareTo(TimeEntry other)
        {
            if (other.Hours == Hours)
                return Minutes.CompareTo(other.Minutes);
            return Hours.CompareTo(other.Hours);
        }

        public override bool Equals(object? obj)
        {
            if (obj is TimeEntry t)
                return Equals(t);
            return false;
        }

        // ReSharper disable RedundantCast
        public override int GetHashCode() => ((int)Hours << 16) | ((int)Minutes << 8) | (int)Seconds;
        // ReSharper restore RedundantCast

        public bool Equals(TimeEntry obj) => obj.Hours == Hours && obj.Minutes == Minutes && obj.Seconds == obj.Seconds;

        public int CompareTo(object? obj)
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

        public override string ToString()
        {
            if (Seconds > 0)
                return $"{Hours:00}:{Minutes:00}:{Seconds:00}";
            return $"{Hours:00}:{Minutes:00}";
        }

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
        public static explicit operator TimeEntry(TimeSpan ts) => new TimeEntry(ts.Days * 24 + ts.Hours, ts.Minutes, ts.Seconds);
    }
}

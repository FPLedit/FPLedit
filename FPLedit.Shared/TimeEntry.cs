using System;
using System.Diagnostics;

namespace FPLedit.Shared
{
    /// <summary>
    /// Represents a single read-only time value, counting hours and minutes.
    /// </summary>
    [Templating.TemplateSafe]
    public readonly struct TimeEntry : IComparable, IComparable<TimeEntry>, IEquatable<TimeEntry>
    {
        public static readonly TimeEntry Zero = new TimeEntry(0,0, 0, 0);

        public short Minutes { get; }
        public short Hours { get; }
        public short Seconds { get; }
        public short Decimals { get; }

        internal TimeEntry(int hours, int minutes, int seconds, int decimals)
        {
            if (seconds != 0 && decimals != 0)
                throw new ArgumentException("Overspecified seconds as well as decimal minutes!");
            
            Seconds = (short)(seconds % 60);
            Decimals = (short)(decimals % 100);

            // Create raw minutes.
            // This already handles all negative seconds/decimals gretaer than 60/100.
            var m = minutes + (seconds - seconds % 60) / 60 + (decimals - decimals % 100) / 100;
            if (Decimals < 0) // |Decimals| < 100
            {
                m--;
                Decimals = (short) (100 + Decimals);
            }
            if (Seconds < 0) // |Seconds| < 60
            {
                m--;
                Seconds = (short) (60 + Seconds);
            }

            Minutes = (short)(m % 60);
            Hours = (short)(hours + (m - m % 60) / 60);
            if (Minutes < 0 && Hours > 0) // |Minutes| < 60
            {
                Hours--;
                Minutes = (short)(60 + Minutes); // Minutes are negative
            }
        }

        public TimeEntry(int hours, int minutes) : this(hours, minutes, 0, 0)
        {
        }
        
        public TimeEntry Add(TimeEntry entry)
        {
            var h = Hours + entry.Hours;
            var m = Minutes + entry.Minutes;
            
            // Exact calculations.
            if (Seconds != 0 && entry.Seconds != 0)
                return new TimeEntry(h, m, Seconds + entry.Seconds, 0);
            if (Decimals != 0 && entry.Decimals != 0)
                return new TimeEntry(h, m, 0, Decimals + entry.Decimals);
            
            // This possibly leads to a loss of precision.
            var d = (GetSecondsDecimal() + entry.GetSecondsDecimal()) * 100M;
            Debug.WriteLineIf(d - (short) d != 0, $"Encountered loss of precision while rounding TimeEntries {d} => {(short)d}", "warning");
            return new TimeEntry(h, m, 0,(short)d);
        }

        public TimeEntry Substract(TimeEntry entry)
        {
            var h = Hours - entry.Hours;
            var m = Minutes - entry.Minutes;
            
            // Exact calculations.
            if (Seconds != 0 && entry.Seconds != 0)
                return new TimeEntry(h, m, Seconds - entry.Seconds, 0);
            if (Decimals != 0 && entry.Decimals != 0)
                return new TimeEntry(h, m, 0, Decimals - entry.Decimals);
            
            // This possibly leads to a loss of precision!
            var d = (GetSecondsDecimal() - entry.GetSecondsDecimal()) * 100M;
            Debug.WriteLineIf(d - (short) d != 0, $"Encountered loss of precision while rounding TimeEntries {d} => {(short)d}", "warning");
            return new TimeEntry(h, m, 0, (short)d);
        }

        public int GetTotalMinutes() => Hours * 60 + Minutes;
        
        public TimeEntry Normalize() => new TimeEntry(Hours % 24, Minutes, Seconds, Decimals);

        private decimal GetSecondsDecimal() => Decimals != 0 ? Decimals / 100M : Seconds / 60M;

        public int CompareTo(TimeEntry other)
        {
            if (other.Hours == Hours)
            {
                if (other.Minutes == Minutes)
                {
                    if (other.Seconds != 0 && Seconds != 0)
                        return Seconds.CompareTo(other.Seconds);
                    if (other.Decimals != 0 && Decimals != 0)
                        return Decimals.CompareTo(other.Decimals);
                    return GetSecondsDecimal().CompareTo(other.GetSecondsDecimal());
                }

                return Minutes.CompareTo(other.Minutes);
            }

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

        public bool Equals(TimeEntry obj) => obj.Hours == Hours && obj.Minutes == Minutes && obj.Seconds == Seconds && obj.Decimals == Decimals;

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

        public override string ToString() => ToTimeString();

        public string ToTimeString(bool allowSeconds = true)
        {
            if (allowSeconds && Seconds > 0)
                return $"{Hours:00}:{Minutes:00}:{Seconds:00}";
            if (allowSeconds && Decimals > 0)
                return $"{Hours:00}:{Minutes:00},{Decimals.ToString().PadRight(2, '"')}";
            return $"{Hours:00}:{Minutes:00}";
        }

        public string ToShortTimeString() => ToTimeString(false);

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
        public static TimeEntry operator *(int n, TimeEntry t) => new TimeEntry(n * t.Hours, n * t.Minutes, n * t.Seconds, n * t.Decimals);
        public static TimeEntry operator *(TimeEntry t, int n) => n * t;
        public static explicit operator TimeEntry(TimeSpan ts) => new TimeEntry(ts.Days * 24 + ts.Hours, ts.Minutes, ts.Seconds, 0);
    }
}

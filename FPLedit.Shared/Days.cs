using System;
using System.Diagnostics;
using System.Linq;

namespace FPLedit.Shared
{
    /// <summary>
    /// Data type, which contains boolean flags for weekdays.
    /// </summary>
    [DebuggerStepThrough]
    [Templating.TemplateSafe]
    public readonly struct Days : IEquatable<Days>, IEquatable<string>
    {
        private readonly bool[] internalDays;

        /// <summary>
        /// Retrieves the boolean flag of the given day. Days are indexed starting with 0 (Monday) until 6 (Sunday).
        /// </summary>
        /// <exception cref="IndexOutOfRangeException">An index lower than 0 or higher than 6 has been used.</exception>
        public bool this[int index]
        {
            get
            {
                if (index < 0 || index > 6)
                    throw new IndexOutOfRangeException("Tage werden von Montag (0) bis Sonntag (7) adressiert!");
                return internalDays[index];
            }
        }

        /// <summary>
        /// Returns the length of the week.
        /// </summary>
        public int Length => 7;

        public Days(bool[] data)
        {
            if (data.Length != 7)
                throw new ArgumentException("Es müssen 7 Tage als bool-Array übergeben werden!");
            internalDays = data;
        }

        /// <summary>
        /// Parse the given binary string representation (7 times 1 or 0, start at Monday) to a <see cref="Days"/> value.
        /// </summary>
        /// <param name="binary">Binary string of length 7.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">The length of the bitsrtring is not 7.</exception>
        public static Days Parse(string binary)
        {
            if (binary.Length != 7)
                throw new ArgumentException("Es müssen 7 Tage als Bitstring übergeben werden!");
            bool[] days = binary.Select(c => c == '1').ToArray();
            return new Days(days);
        }

        /// <summary>
        /// Generates a seven-bit binary string represenatation of the current Days value.
        /// </summary>
        public string ToBinString() 
            => internalDays.Aggregate("", (current, t) => current + (t ? "1" : "0"));

        public string DaysToString(bool veryShort = false)
        {
            if (veryShort)
            {
                if (EqualsString("1111111")) // Mo - So
                    return "";
                if (EqualsString("1111110")) // Mo - Sa => W
                    return "W";
                if (EqualsString("1111100")) // Mo - Fr => W [Sa]
                    return "W [Sa]";
                if (EqualsString("0000001")) // So
                    return "S";
            }

            string[] str = new string[7];
            str[0] = this[0] ? "Mo" : null;
            str[1] = this[1] ? "Di" : null;
            str[2] = this[2] ? "Mi" : null;
            str[3] = this[3] ? "Do" : null;
            str[4] = this[4] ? "Fr" : null;
            str[5] = this[5] ? "Sa" : null;
            str[6] = this[6] ? "So" : null;

            return string.Join(", ", str.Where(o => o != null));
        }

        /// <summary>
        /// Returns whether this intance and <paramref name="daysB"/> have days in common.
        /// </summary>
        /// <seealso cref="IntersectingDays"/>
        public bool IsIntersecting(Days daysB)
        {
            for (int i = 0; i < 7; i++)
                if (this[i] && daysB[i])
                    return true;

            return false;
        }

        /// <summary>
        /// Retunrns the days, that this instance and <paramref name="daysB"/> have in common.
        /// </summary>
        /// <seealso cref="IsIntersecting"/>
        public Days IntersectingDays(Days daysB)
        {
            var res = new bool[7];
            for (int i = 0; i < 7; i++)
                if (this[i] && daysB[i])
                    res[i] = true;
            return new Days(res);
        }
        
        /// <summary>
        /// Returns, whether the given binary string representation equals this Days instance.
        /// </summary>
        /// <param name="compare"></param>
        /// <returns></returns>
        private bool EqualsString(string compare) => Parse(compare).Equals(this);

        public override bool Equals(object obj)
            => obj is Days d && internalDays.SequenceEqual(d.internalDays);

        public override int GetHashCode()
        {
            var ret = 0;
            for (var i = 0; i < 7; i++)
                ret |= (internalDays[i] ? 1 : 0) << i;

            return ret;
        }

        public bool Equals(Days other) => Equals((object)other);

        public bool Equals(string other) => EqualsString(other);

        public static bool operator !=(Days d1, Days d2) => !d1.Equals(d2);

        public static bool operator ==(Days d1, Days d2) => d1.Equals(d2);
    }
}

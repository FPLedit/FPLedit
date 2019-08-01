using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace FPLedit.Shared
{
    [DebuggerStepThrough]
    public struct Days : IEquatable<Days>, IEquatable<string>
    {
        private readonly bool[] internalDays;

        public bool this[int index]
        {
            get
            {
                if (index < 0 || index > 6)
                    throw new IndexOutOfRangeException("Tage werden von Montag (0) bis Sonntag (7) adressiert!");
                return internalDays[index];
            }
        }

        public int Length => 7;

        public Days(bool[] data)
        {
            if (data.Length != 7)
                throw new ArgumentException("Es müssen 7 Tage als bool-Array übergeben werden!");
            internalDays = data;
        }

        public static Days Parse(string binary)
        {
            if (binary.Length != 7)
                throw new ArgumentException("Es müssen 7 Tage als Bitstring übergeben werden!");
            bool[] days = binary.Select(c => c == '1').ToArray();
            return new Days(days);
        }

        public string ToBinString()
        {
            string ret = "";
            for (int i = 0; i < internalDays.Length; i++)
                ret += internalDays[i] ? "1" : "0";
            return ret;
        }

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

        public bool IsIntersecting(Days daysB)
        {
            for (int i = 0; i < 7; i++)
                if (this[i] && daysB[i])
                    return true;

            return false;
        }

        public Days IntersectingDays(Days daysB)
        {
            var res = new bool[7];
            for (int i = 0; i < 7; i++)
                if (this[i] && daysB[i])
                    res[i] = true;
            return new Days(res);
        }

        public override bool Equals(object obj)
            => obj is Days d && internalDays.SequenceEqual(d.internalDays);

        public override int GetHashCode()
        {
            var ret = 0;
            for (int i = 0; i < 7; i++)
                ret |= (internalDays[i] ? 1 : 0) << i;

            return ret;
        }

        private bool EqualsString(string compare) => Parse(compare).Equals(this);

        public bool Equals(Days other) => Equals((object)other);

        public bool Equals(string other) => EqualsString(other);

        public static bool operator !=(Days d1, Days d2) => !d1.Equals(d2);

        public static bool operator ==(Days d1, Days d2) => d1.Equals(d2);
    }
}

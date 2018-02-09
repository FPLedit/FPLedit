using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace FPLedit.Shared.Helpers
{
    public static class DaysHelper
    {
        [DebuggerStepThrough]
        public static bool[] ParseDays(string binary)
        {
            bool[] days = new bool[7];
            char[] chars = binary.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
                days[i] = chars[i] == '1';
            return days;
        }

        [DebuggerStepThrough]
        public static string DaysToBinString(bool[] days)
        {
            string ret = "";
            for (int i = 0; i < days.Length; i++)
                ret += days[i] ? "1" : "0";
            return ret;
        }

        [DebuggerStepThrough]
        public static string DaysToString(bool[] days, bool veryShort = false)
        {
            string[] str = new string[7];
            if (!veryShort) {
                str[0] = days[0] ? "Mo" : null;
                str[1] = days[1] ? "Di" : null;
                str[2] = days[2] ? "Mi" : null;
                str[3] = days[3] ? "Do" : null;
                str[4] = days[4] ? "Fr" : null;
                str[5] = days[5] ? "Sa" : null;
                str[6] = days[6] ? "So" : null;
            }
            else
            {
                if (days.All(d => d == true)) // Mo - So
                    return "";
                if (days.Take(6).All(d => d == true) && !days[6]) // Mo - Sa => W
                    return "W";
                if (days.Take(5).All(d => d == true) && !days[5] && !days[6]) // Mo - Fr => W (Sa)
                    return "W (Sa)";
                if (days.Take(6).All(d => d == false) && days[6]) // So
                    return "So";
                return DaysToString(days);
            }

            return string.Join(", ", str.Where(o => o != null));
        }

        [DebuggerStepThrough]
        public static bool IntersectDays(bool[] daysA, bool[] daysB)
        {
            for (int i = 0; i < 7; i++)
                if (daysA[i] && daysB[i])
                    return true;

            return false;
        }

        [DebuggerStepThrough]
        public static bool[] IntersectingDays(bool[] daysA, bool[] daysB)
        {
            var res = new bool[7];
            for (int i = 0; i < 7; i++)
                if (daysA[i] && daysB[i])
                    res[i] = true;
            return res;
        }
    }
}

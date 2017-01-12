using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FPLedit.Shared
{
    public class DaysHelper
    {
        public static bool[] ParseDays(string binary)
        {
            bool[] days = new bool[7];
            char[] chars = binary.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
                days[i] = chars[i] == '1';
            return days;
        }

        public static string DaysToBinString(bool[] days)
        {
            string ret = "";
            for (int i = 0; i < days.Length; i++)
                ret += days[i] ? "1" : "0";
            return ret;
        }

        public static string DaysToString(bool[] days)
        {
            string[] str = new string[7];
            str[0] = days[0] ? "Mo" : null;
            str[1] = days[1] ? "Di" : null;
            str[2] = days[2] ? "Mi" : null;
            str[3] = days[3] ? "Do" : null;
            str[4] = days[4] ? "Fr" : null;
            str[5] = days[5] ? "Sa" : null;
            str[6] = days[6] ? "So" : null;

            return string.Join(", ", str.Where(o => o != null));
        }        
    }
}

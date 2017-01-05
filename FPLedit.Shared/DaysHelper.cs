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
            str[0] = days[0] ? "Montag" : null;
            str[1] = days[1] ? "Dienstag" : null;
            str[2] = days[2] ? "Mittwoch" : null;
            str[3] = days[3] ? "Donnerstag" : null;
            str[4] = days[4] ? "Freitag" : null;
            str[5] = days[5] ? "Samstag" : null;
            str[6] = days[6] ? "Sonntag" : null;

            return string.Join(", ", str.Where(o => o != null));
        }        
    }
}

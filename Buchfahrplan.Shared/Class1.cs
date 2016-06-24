using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Buchfahrplan.Shared
{
    public static class Meta
    {
        public static T Parse<T>(string input, Func<string, T> func)
        {
            return func(input);
        }

        public static string GetMeta(Timetable tt, string key, string defaultValue)
        {
            if (tt.Metadata.ContainsKey(key))
                return tt.Metadata[key];
            else
                return defaultValue;
        }

        public static T GetMeta<T>(Timetable tt, string key, T defaultValue, Func<string, T> func)
        {
            if (tt.Metadata.ContainsKey(key))
                return func(tt.Metadata[key]);
            else
                return defaultValue;
        }
    }
}

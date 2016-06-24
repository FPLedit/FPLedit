using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Buchfahrplan.Shared
{
    [Serializable]
    public sealed class Train
    {
        public string Name { get; set; }

        public string Line { get; set; }

        public Dictionary<Station, TimeSpan> Arrivals { get; set; }

        public Dictionary<Station, TimeSpan> Departures { get; set; }

        public string Locomotive { get; set; }

        public bool Direction { get; set; }

        public bool[] Days { get; set; }

        public Dictionary<string, string> Metadata { get; set; }

        public Train()
        {
            Arrivals = new Dictionary<Station, TimeSpan>();
            Departures = new Dictionary<Station, TimeSpan>();
            Metadata = new Dictionary<string, string>();
            Days = new bool[7];
        }

        public override string ToString()
        {
            return Name;
        }

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
    }
}

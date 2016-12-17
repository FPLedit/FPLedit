using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace FPLedit.Shared
{
    [Serializable]
    public sealed class Train : Entity
    {
        public string Name
        {
            get
            {
                return GetAttribute<string>("name");
            }
            set
            {
                SetAttribute("name", value);
            }
        }

        public string Line { get; set; }

        //public Dictionary<Station, TimeSpan> Arrivals { get; set; }

        //public Dictionary<Station, TimeSpan> Departures { get; set; }

        public Dictionary<Station, ArrDep> ArrDeps { get; set; }

        public string Locomotive
        {
            get
            {
                return GetAttribute<string>("tfz", "");
            }
            set
            {
                SetAttribute("tfz", value);
            }
        }

        public TrainDirection Direction { get; set; }

        public bool[] Days
        {
            get
            {
                var d = GetAttribute<string>("d", "1111111");
                return ParseDays(d);
            }
            set
            {
                var d = DaysToBinString(value);
                SetAttribute("d", d);
            }
        }

        public Train() : base()
        {
            //Arrivals = new Dictionary<Station, TimeSpan>();
            //Departures = new Dictionary<Station, TimeSpan>();
            ArrDeps = new Dictionary<Station, ArrDep>();
            Line = "";
        }

        public void InitializeStations(Timetable tt)
        {
            var stas = tt.GetStationsOrderedByDirection(Direction)
                .Skip(1); // Remove first station (only departure)

            foreach (var sta in stas)
                ArrDeps.Add(sta, new ArrDep());
        }

        [DebuggerStepThrough]
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

        public string DaysToString()
        {
            return DaysToString(Days);
        }        
    }    
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace FPLedit.Shared
{
    [Serializable]
    public sealed class Train : XMLEntity
    {
        public string TName
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

        public TrainDirection Direction
        {
            get
            {
                return XName == "ti" ? TrainDirection.ti : TrainDirection.ta;
            }
        }

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

        public Train(TrainDirection dir) : base(dir.ToString())
        {

        }

        public Train(XMLEntity en, List<Station> stas) : base(en.el)
        {
            ArrDeps = new Dictionary<Station, ArrDep>();

            int i = 0;
            foreach (var time in en.Children.Where(x => x.XName == "t"))
            {
                ArrDep ardp = new ArrDep();
                if (time.GetAttribute("a", "") != "")
                    ardp.Arrival = TimeSpan.Parse(time.GetAttribute<string>("a"));

                if (time.GetAttribute("d", "") != "")
                    ardp.Departure = TimeSpan.Parse(time.GetAttribute<string>("d"));
                ArrDeps[stas.ElementAt(i)] = ardp;
                i++;
            }
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
            return TName;
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

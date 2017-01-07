using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace FPLedit.Shared
{
    [Serializable]
    public sealed class Train : Entity
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

        public void AddArrDep(Station sta, ArrDep ardp)
        {
            var stas = _parent.Stations.OrderBy(s => s.Kilometre).ToList();
            var idx = stas.IndexOf(sta);

            var ar = ardp.Arrival.ToShortTimeString();
            var dp = ardp.Departure.ToShortTimeString();
            
            var tElm = new XMLEntity("t");
            tElm.SetAttribute("a", ar != "00:00" ? ar : "");
            tElm.SetAttribute("d", dp != "00:00" ? dp : "");
            Children.Insert(idx, tElm);
        }

        public void SetArrDep(Station sta, ArrDep ardp)
        {
            var stas = _parent.Stations.OrderBy(s => s.Kilometre).ToList();
            var idx = stas.IndexOf(sta);
            var tElm = Children.Where(x => x.XName == "t").ToList()[idx];

            var ar = ardp.Arrival.ToShortTimeString();
            var dp = ardp.Departure.ToShortTimeString();
            tElm.SetAttribute("a", ar != "00:00" ? ar : "");
            tElm.SetAttribute("d", dp != "00:00" ? dp : "");
        }

        public ArrDep GetArrDep(Station sta)
        {
            var stas = _parent.Stations.OrderBy(s => s.Kilometre).ToList();
            var idx = stas.IndexOf(sta);
            var tElm = Children.Where(x => x.XName == "t").ToList()[idx];

            ArrDep ardp = new ArrDep();

            if (tElm.GetAttribute("a", "") != "")
                ardp.Arrival = TimeSpan.Parse(tElm.GetAttribute<string>("a"));

            if (tElm.GetAttribute("d", "") != "")
                ardp.Departure = TimeSpan.Parse(tElm.GetAttribute<string>("d"));

            return ardp;
        }

        public string Locomotive
        {
            get
            {
                return GetAttribute<string>("fpl-tfz", "");
            }
            set
            {
                SetAttribute("fpl-tfz", value);
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
                return DaysHelper.ParseDays(d);
            }
            set
            {
                var d = DaysHelper.DaysToBinString(value);
                SetAttribute("d", d);
            }
        }

        public Train(TrainDirection dir, Timetable tt) : base(dir.ToString(), tt)
        {
        }

        public Train(XMLEntity en, Timetable tt) : base(en, tt)
        {
        }

        [DebuggerStepThrough]
        public override string ToString()
        {
            return TName;
        }

        [DebuggerStepThrough]
        public string DaysToString()
        {
            return DaysHelper.DaysToString(Days);
        }
    }    
}

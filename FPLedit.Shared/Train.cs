using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace FPLedit.Shared
{
    [Serializable]
    public sealed class Train : XMLEntity
    {
        private Timetable _parent;

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

        //private Dictionary<Station, ArrDep> ArrDeps { get; set; }

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

            //ArrDeps[sta] = ardp;
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
                return DaysHelper.ParseDays(d);
            }
            set
            {
                var d = DaysHelper.DaysToBinString(value);
                SetAttribute("d", d);
            }
        }

        public Train(TrainDirection dir, Timetable tt) : base(dir.ToString())
        {
            _parent = tt;
        }

        public Train(XMLEntity en, List<Station> stas, Timetable tt) : base(en.el)
        {
            _parent = tt;
            //ArrDeps = new Dictionary<Station, ArrDep>();

            //TODO: Serialize back into xml
            //int i = 0;
            //foreach (var time in en.Children.Where(x => x.XName == "t"))
            //{
            //    ArrDep ardp = new ArrDep();
            //    if (time.GetAttribute("a", "") != "")
            //        ardp.Arrival = TimeSpan.Parse(time.GetAttribute<string>("a"));

            //    if (time.GetAttribute("d", "") != "")
            //        ardp.Departure = TimeSpan.Parse(time.GetAttribute<string>("d"));
            //    SetArrDep(stas.ElementAt(i), ardp);
            //    i++;
            //}
        }

        //public void InitializeStations(Timetable tt)
        //{
        //    var stas = tt.GetStationsOrderedByDirection(Direction)
        //        .Skip(1); // Remove first station (only departure)

        //    foreach (var sta in tt.Stations)
        //        AddArrDep(sta, new ArrDep());
        //}

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

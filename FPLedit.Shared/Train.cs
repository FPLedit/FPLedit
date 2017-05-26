using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace FPLedit.Shared
{
    [Serializable]
    [DebuggerDisplay("{TName}")]
    public sealed class Train : Entity
    {
        public string TName
        {
            get => GetAttribute<string>("name");
            set => SetAttribute("name", value);
        }

        #region Handling der Fahrtzeiteneinträge

        public void AddArrDep(Station sta, ArrDep ardp)
        {
            var stas = _parent.Stations.OrderBy(s => s.Kilometre).ToList();
            var idx = stas.IndexOf(sta);

            var ar = ardp.Arrival.ToShortTimeString();
            var dp = ardp.Departure.ToShortTimeString();

            var tElm = new XMLEntity("t");
            tElm.SetAttribute("a", ar != "00:00" ? ar : "");
            tElm.SetAttribute("d", dp != "00:00" ? dp : "");
            tElm.SetAttribute("fpl-tr", ardp.TrapeztafelHalt ? "1" : "0");
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
            tElm.SetAttribute("fpl-tr", ardp.TrapeztafelHalt ? "1" : "0");
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

            if (tElm.GetAttribute("fpl-tr", "") != "")
                ardp.TrapeztafelHalt = Convert.ToBoolean(tElm.GetAttribute<int>("fpl-tr"));

            return ardp;
        }

        public void RemoveArrDep(Station sta)
        {
            var stas = _parent.Stations.OrderBy(s => s.Kilometre).ToList();
            var idx = stas.IndexOf(sta);
            var tElm = Children.Where(x => x.XName == "t").ToList()[idx];

            Children.Remove(tElm);
        }

        public void RemovedOrphanedTimes()
        {
            // Räumt verwaiste Zeiten auf (z.B. Ankunftszeit im Startbahnhof)
            var stas = _parent.GetStationsOrderedByDirection(Direction);

            if (stas.Count == 0) // Die letzte Station wurde gelöscht
                return;

            var fs = stas.First();
            var ls = stas.Last();

            var fa = GetArrDep(fs);
            var la = GetArrDep(ls);

            fa.Arrival = default(TimeSpan);
            la.Departure = default(TimeSpan);

            SetArrDep(fs, fa);
            SetArrDep(ls, la);
        }

        #endregion

        public string Locomotive
        {
            get => GetAttribute<string>("fpl-tfz", "");
            set => SetAttribute("fpl-tfz", value);
        }

        public TrainDirection Direction => XName == "ti" ? TrainDirection.ti : TrainDirection.ta;

        public string Comment
        {
            get => GetAttribute<string>("cm", "");
            set => SetAttribute("cm", value);
        }

        public bool[] Days
        {
            get => DaysHelper.ParseDays(GetAttribute("d", "1111111"));
            set => SetAttribute("d", DaysHelper.DaysToBinString(value));
        }

        public Train(TrainDirection dir, Timetable tt) : base(dir.ToString(), tt)
        {
        }

        public Train(XMLEntity en, Timetable tt) : base(en, tt)
        {
            if (Children.Where(x => x.XName == "t").Count() > tt.Stations.Count)
                throw new Exception("Zu viele Fahrtzeiteneinträge im Vergleich zur Stationsanzahl!");
        }

        [DebuggerStepThrough]
        public override string ToString()
            => TName;
    }
}

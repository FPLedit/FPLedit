using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace FPLedit.Shared
{
    [Serializable]
    [DebuggerDisplay("{TName}")]
    public sealed class Train : Entity, ITrain
    {
        public string TName
        {
            get => GetAttribute<string>("name");
            set => SetAttribute("name", value);
        }

        public int Id
        {
            get => GetAttribute<int>("id", -1);
            set => SetAttribute("id", value.ToString());
        }

        public bool IsLink
        {
            get => GetAttribute<bool>("islink");
            set => SetAttribute("islink", value.ToString());
        }

        #region Handling der Fahrtzeiteneinträge

        public void AddAllArrDeps(List<Station> path)
        {
            if (_parent.Type != TimetableType.Network)
                throw new Exception("Lineare Fahrpläne haben keine Laufwege!");
            foreach (var sta in path)
            {
                var tElm = new XMLEntity("t");
                tElm.SetAttribute("fpl-id", sta.Id.ToString());
                tElm.SetAttribute("a", "");
                tElm.SetAttribute("d", "");
                tElm.SetAttribute("fpl-tr", "0");
                tElm.SetAttribute("fpl-zlm", "");
                Children.Add(tElm);
            }
        }

        public void AddLinearArrDeps()
        {
            if (_parent.Type == TimetableType.Network)
                throw new Exception("Bei Netzwerk-Fahrplänen nicht möglich!");
            foreach (var sta in _parent.Stations)
                AddArrDep(sta, new ArrDep(), Timetable.LINEAR_ROUTE_ID);
        }


        public List<Station> GetPath()
        {
            if (_parent.Type == TimetableType.Network)
            {
                List<Station> ret = new List<Station>();
                var tElems = Children.Where(x => x.XName == "t").ToList();
                foreach (var tElm in tElems)
                {
                    var sta = _parent.Stations.FirstOrDefault(s => s.Id == tElm.GetAttribute<int>("fpl-id"));
                    ret.Add(sta);
                }
                return ret;
            }
            else
                return _parent.GetStationsOrderedByDirection(Direction);
        }

        public void AddArrDep(Station sta, ArrDep ardp, int route)
        {
            int idx;
            if (_parent.Type == TimetableType.Linear)
            {
                var stas = _parent.Stations.OrderBy(s => s.LinearKilometre).ToList();
                idx = stas.IndexOf(sta);
            }
            else
            {
                var r = _parent.GetRoute(route).GetOrderedStations();
                var i1 = r.IndexOf(sta);
                var p = GetPath();
                Station prev = null, next = null;
                if (r.ElementAtOrDefault(i1 - 1) != null)
                    prev = r.ElementAtOrDefault(i1 - 1);
                if (r.ElementAtOrDefault(i1 + 1) != null)
                    next = r.ElementAtOrDefault(i1 + 1);

                if (prev != null && p.Contains(prev) && next != null && p.Contains(next))
                    idx = p.IndexOf(prev) + 1;
                else
                    return; // Betrifft diesen Zug nicht
            }

            var ar = ardp.Arrival.ToShortTimeString();
            var dp = ardp.Departure.ToShortTimeString();

            var tElm = new XMLEntity("t");
            tElm.SetAttribute("a", ar != "00:00" ? ar : "");
            tElm.SetAttribute("d", dp != "00:00" ? dp : "");
            tElm.SetAttribute("fpl-tr", ardp.TrapeztafelHalt ? "1" : "0");
            tElm.SetAttribute("fpl-zlm", ardp.Zuglaufmeldung);
            if (_parent.Type == TimetableType.Network)
                tElm.SetAttribute("fpl-id", sta.Id.ToString());
            Children.Insert(idx, tElm);
        }

        public void SetArrDep(Station sta, ArrDep ardp)
        {
            var tElems = Children.Where(x => x.XName == "t").ToList();
            XMLEntity tElm;
            if (_parent.Type == TimetableType.Linear)
            {
                var stas = _parent.Stations.OrderBy(s => s.LinearKilometre).ToList();
                var idx = stas.IndexOf(sta);
                tElm = tElems[idx];
            }
            else
                tElm = tElems.First(t => t.GetAttribute<int>("fpl-id") == sta.Id);

            var ar = ardp.Arrival.ToShortTimeString();
            var dp = ardp.Departure.ToShortTimeString();
            tElm.SetAttribute("a", ar != "00:00" ? ar : "");
            tElm.SetAttribute("d", dp != "00:00" ? dp : "");
            tElm.SetAttribute("fpl-tr", ardp.TrapeztafelHalt ? "1" : "0");
            tElm.SetAttribute("fpl-zlm", ardp.Zuglaufmeldung);
        }

        public ArrDep GetArrDep(Station sta)
        {
            var tElems = Children.Where(x => x.XName == "t").ToList();
            XMLEntity tElm;
            if (_parent.Type == TimetableType.Linear)
            {
                var stas = _parent.Stations.OrderBy(s => s.LinearKilometre).ToList();
                var idx = stas.IndexOf(sta);
                tElm = tElems[idx];
            }
            else
                tElm = tElems.First(t => t.GetAttribute<int>("fpl-id") == sta.Id);

            ArrDep ardp = new ArrDep();

            if (tElm.GetAttribute("a", "") != "")
                ardp.Arrival = TimeSpan.Parse(tElm.GetAttribute<string>("a"));

            if (tElm.GetAttribute("d", "") != "")
                ardp.Departure = TimeSpan.Parse(tElm.GetAttribute<string>("d"));

            if (tElm.GetAttribute("fpl-tr", "") != "")
                ardp.TrapeztafelHalt = Convert.ToBoolean(tElm.GetAttribute<int>("fpl-tr"));
            if (tElm.GetAttribute("fpl-zlm", "") != "")
                ardp.Zuglaufmeldung = tElm.GetAttribute<string>("fpl-zlm");

            return ardp;
        }

        public Dictionary<Station, ArrDep> GetArrDeps()
        {
            var ret = new Dictionary<Station, ArrDep>();
            var tElm = Children.Where(x => x.XName == "t").ToList();
            foreach (var t in tElm)
            {
                ArrDep ardp = new ArrDep();
                var sta = _parent.Type == TimetableType.Network
                    ? _parent.GetStationById(t.GetAttribute<int>("fpl-id"))
                    : _parent.Stations.OrderBy(s => s.LinearKilometre).ToList()[tElm.IndexOf(t)];

                if (t.GetAttribute("a", "") != "")
                    ardp.Arrival = TimeSpan.Parse(t.GetAttribute<string>("a"));

                if (t.GetAttribute("d", "") != "")
                    ardp.Departure = TimeSpan.Parse(t.GetAttribute<string>("d"));

                if (t.GetAttribute("fpl-tr", "") != "")
                    ardp.TrapeztafelHalt = Convert.ToBoolean(t.GetAttribute<int>("fpl-tr"));
                if (t.GetAttribute("fpl-zlm", "") != "")
                    ardp.Zuglaufmeldung = t.GetAttribute<string>("fpl-zlm");
                ret.Add(sta, ardp);
            }
            return ret;
        }

        public void RemoveArrDep(Station sta)
        {
            var tElems = Children.Where(x => x.XName == "t").ToList();
            XMLEntity tElm;
            if (_parent.Type == TimetableType.Linear)
            {
                var stas = _parent.Stations.OrderBy(s => s.LinearKilometre).ToList();
                var idx = stas.IndexOf(sta);
                tElm = tElems[idx];
            }
            else
                tElm = tElems.FirstOrDefault(t => t.GetAttribute<int>("fpl-id") == sta.Id);

            Children.Remove(tElm);
        }

        public void RemoveOrphanedTimes()
        {
            // Räumt verwaiste Zeiten auf (z.B. Ankunftszeit im Startbahnhof)
            var stas = _parent.Type == TimetableType.Linear
                ? _parent.Stations.OrderBy(s => s.LinearKilometre).ToList()
                : GetPath();

            if (stas.Count == 0) // Die letzte Station wurde gelöscht
                return;

            var fs = stas.First();
            var ls = stas.Last();

            var fa = GetArrDep(fs);
            var la = GetArrDep(ls);

            fa.Arrival = default;
            la.Departure = default;

            SetArrDep(fs, fa);
            SetArrDep(ls, la);
        }

        #endregion

        public string Locomotive
        {
            get => GetAttribute("fpl-tfz", "");
            set => SetAttribute("fpl-tfz", value);
        }

        public string Mbr
        {
            get => GetAttribute("fpl-mbr", "");
            set => SetAttribute("fpl-mbr", value);
        }

        public string Last
        {
            get => GetAttribute("fpl-last", "");
            set => SetAttribute("fpl-last", value);
        }

        public TrainDirection Direction => (TrainDirection)Enum.Parse(typeof(TrainDirection), XMLEntity.XName);

        public string Comment
        {
            get => GetAttribute("cm", "");
            set => SetAttribute("cm", value);
        }

        public Days Days
        {
            get => Days.Parse(GetAttribute("d", "1111111"));
            set => SetAttribute("d", value.ToBinString());
        }

        public Train(TrainDirection dir, Timetable tt) : base(dir.ToString(), tt)
        {
            if (tt.Type == TimetableType.Network && dir != TrainDirection.tr)
                throw new NotSupportedException("Netzwerk-Fahrpläne haben keine gerichteten Züge");
            else if (tt.Type == TimetableType.Linear && dir == TrainDirection.tr)
                throw new NotSupportedException("Lineare Fahrpläne haben haben nur gerichtete Züge");
        }

        public Train(XMLEntity en, Timetable tt) : base(en, tt)
        {
            if (Children.Count(x => x.XName == "t") > tt.Stations.Count)
                throw new Exception("Zu viele Fahrtzeiteneinträge im Vergleich zur Stationsanzahl!");
        }

        [DebuggerStepThrough]
        public override string ToString()
            => TName;

        public string GetLineName()
        {
            var path = GetPath();
            return path.First().SName + " - " + path.Last().SName;
        }
    }
}

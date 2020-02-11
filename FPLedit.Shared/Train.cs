using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace FPLedit.Shared
{
    [DebuggerDisplay("{" + nameof(TName) + "}")]
    [XElmName("ti", "ta", "tr")]
    [Templating.TemplateSafe]
    public sealed class Train : Entity, ITrain
    {
        [XAttrName("name")]
        public string TName
        {
            get => GetAttribute<string>("name");
            set => SetAttribute("name", value);
        }

        [XAttrName("id")]
        public int Id
        {
            get => GetAttribute<int>("id", -1);
            set => SetAttribute("id", value.ToString());
        }

        [XAttrName("islink")]
        public bool IsLink
        {
            get => GetAttribute<bool>("islink");
            set => SetAttribute("islink", value.ToString());
        }

        #region Handling der Fahrtzeiteneinträge

        public void AddAllArrDeps(IEnumerable<Station> path)
        {
            if (_parent.Type != TimetableType.Network)
                throw new Exception("Lineare Fahrpläne haben keine Laufwege!");
            foreach (var sta in path)
            {
                var ardp = new ArrDep(_parent)
                {
                    StationId = sta.Id
                };
                Children.Add(ardp.XMLEntity);
            }
        }

        public void AddLinearArrDeps()
        {
            if (_parent.Type == TimetableType.Network)
                throw new Exception("Bei Netzwerk-Fahrplänen nicht möglich!");
            foreach (var sta in _parent.Stations)
                AddArrDep(sta, Timetable.LINEAR_ROUTE_ID);
        }

        public List<Station> GetPath()
        {
            if (_parent.Type == TimetableType.Network)
                return InternalGetArrDeps().Select(ardp => _parent.GetStationById(ardp.StationId)).ToList();
            return _parent.GetStationsOrderedByDirection(Direction);
        }

        public ArrDep AddArrDep(Station sta, int route)
        {
            int idx;
            if (_parent.Type == TimetableType.Linear)
            {
                var stas = _parent.GetStationsOrderedByDirection();
                idx = stas.IndexOf(sta);
            }
            else
            {
                var r = _parent.GetRoute(route).Stations;
                var i1 = r.IndexOf(sta);
                var p = GetPath();
                var prev = r.ElementAtOrDefault(i1 - 1);
                var next = r.ElementAtOrDefault(i1 + 1);

                if (prev != null && p.Contains(prev) && next != null && p.Contains(next))
                    idx = p.IndexOf(prev) + 1;
                else
                    return null; // Betrifft diesen Zug nicht (wenn sta letzte/erste Station der Route: der Zug befährt die Route nur bis davor/danach)
            }

            var ardp = new ArrDep(_parent);
            if (_parent.Type == TimetableType.Network)
                ardp.StationId = sta.Id;
            Children.Insert(idx, ardp.XMLEntity);
            return ardp;
        }

        public ArrDep GetArrDep(Station sta)
        {
            if (TryGetArrDep(sta, out var arrDep))
                return arrDep;
            throw new Exception($"No ArrDep found for station {sta.SName}!");
        }

        public bool TryGetArrDep(Station sta, out ArrDep arrDep)
        {
            var tElems = InternalGetArrDeps();
            if (_parent.Type == TimetableType.Linear)
            {
                var stas = _parent.GetStationsOrderedByDirection();
                var idx = stas.IndexOf(sta);
                arrDep = tElems[idx];
                return true;
            }
            arrDep = tElems.FirstOrDefault(t => t.StationId == sta.Id);
            return arrDep != null;
        }

        public Dictionary<Station, ArrDep> GetArrDeps()
        {
            var ret = new Dictionary<Station, ArrDep>();
            var ardps = InternalGetArrDeps();
            foreach (var ardp in ardps)
            {
                var sta = _parent.Type == TimetableType.Network
                    ? _parent.GetStationById(ardp.StationId)
                    : _parent.GetStationsOrderedByDirection()[Array.IndexOf(ardps, ardp)];

                if (ret.ContainsKey(sta))
                    throw new Exception($"The path already contains the station \"{sta.SName}\"");

                ret.Add(sta, ardp);
            }
            return ret;
        }

        public void RemoveArrDep(Station sta)
        {
            var tElems = InternalGetArrDeps();
            ArrDep tElm;
            if (_parent.Type == TimetableType.Linear)
            {
                var stas = _parent.GetStationsOrderedByDirection();
                var idx = stas.IndexOf(sta);
                tElm = tElems[idx];
            }
            else
                tElm = tElems.FirstOrDefault(t => t.StationId == sta.Id);

            if (tElm == null)
                return;

            Children.Remove(tElm.XMLEntity);
        }

        public void RemoveOrphanedTimes()
        {
            // Räumt verwaiste Zeiten auf (z.B. Ankunftszeit im Startbahnhof)
            var stas = _parent.Type == TimetableType.Linear
                ? _parent.GetStationsOrderedByDirection()
                : GetPath();

            if (stas.Count == 0) // Die letzte Station wurde gelöscht
                return;

            var fs = stas.First();
            var ls = stas.Last();

            GetArrDep(fs).Arrival = default;
            GetArrDep(ls).Departure = default;
        }

        private ArrDep[] InternalGetArrDeps() => Children.Where(x => x.XName == "t").Select(x => new ArrDep(x, _parent)).ToArray();

        #endregion

        [XAttrName("fpl-tfz", IsFpleditElement = true)]
        public string Locomotive
        {
            get => GetAttribute("fpl-tfz", "");
            set => SetAttribute("fpl-tfz", value);
        }

        [XAttrName("fpl-mbr", IsFpleditElement = true)]
        public string Mbr
        {
            get => GetAttribute("fpl-mbr", "");
            set => SetAttribute("fpl-mbr", value);
        }

        [XAttrName("fpl-last", IsFpleditElement = true)]
        public string Last
        {
            get => GetAttribute("fpl-last", "");
            set => SetAttribute("fpl-last", value);
        }

        public TrainDirection Direction => (TrainDirection)Enum.Parse(typeof(TrainDirection), XMLEntity.XName);

        [XAttrName("cm")]
        public string Comment
        {
            get => GetAttribute("cm", "");
            set => SetAttribute("cm", value);
        }

        [XAttrName("d")]
        public Days Days
        {
            get => Days.Parse(GetAttribute("d", "1111111"));
            set => SetAttribute("d", value.ToBinString());
        }

        public Train(TrainDirection dir, Timetable tt) : base(dir.ToString(), tt)
        {
            if (tt.Type == TimetableType.Network && dir != TrainDirection.tr)
                throw new NotSupportedException("Netzwerk-Fahrpläne haben keine gerichteten Züge");
            if (tt.Type == TimetableType.Linear && dir == TrainDirection.tr)
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

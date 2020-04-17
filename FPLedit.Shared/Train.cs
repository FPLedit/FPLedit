using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace FPLedit.Shared
{
    /// <summary>
    /// Object model class representing a single train of this timetable.
    /// </summary>
    [DebuggerDisplay("{" + nameof(TName) + "}")]
    [XElmName("ti", "ta", "tr")]
    [Templating.TemplateSafe]
    public sealed class Train : Entity, IWritableTrain
    {
        /// <inheritdoc />
        [XAttrName("name")]
        public string TName
        {
            get => GetAttribute<string>("name");
            set => SetAttribute("name", value);
        }

        /// <inheritdoc />
        [XAttrName("id")]
        public int Id
        {
            get => GetAttribute<int>("id", -1);
            set => SetAttribute("id", value.ToString());
        }

        /// <inheritdoc />
        [XAttrName("islink")]
        public bool IsLink => false;

        #region Handling der Fahrtzeiteneinträge

        /// <inheritdoc />
        public void AddAllArrDeps(IEnumerable<Station> path)
        {
            if (_parent.Type != TimetableType.Network)
                throw new TimetableTypeNotSupportedException(TimetableType.Linear, "AddAllArrDeps of path");
            if (InternalGetArrDeps().Any())
                throw new InvalidOperationException("Train.AddAllArrDeps can only be used on empty trains.");
            foreach (var sta in path)
            {
                var ardp = new ArrDep(_parent)
                {
                    StationId = sta.Id
                };
                Children.Add(ardp.XMLEntity);
            }
        }

        /// <inheritdoc />
        public void AddLinearArrDeps()
        {
            if (_parent.Type == TimetableType.Network)
                throw new TimetableTypeNotSupportedException(TimetableType.Network, "Add linear ArrDeps");
            if (InternalGetArrDeps().Any())
                throw new InvalidOperationException("Train.AddLinearArrDeps can only be used on empty trains.");
            foreach (var sta in _parent.GetLinearStationsOrderedByDirection(TrainDirection.ti)) // All arrdeps are sorted in line direction if linear
                AddArrDep(sta, Timetable.LINEAR_ROUTE_ID);
        }

        /// <inheritdoc />
        public List<Station> GetPath()
        {
            if (_parent.Type == TimetableType.Network)
                return InternalGetArrDeps().Select(ardp => _parent.GetStationById(ardp.StationId)).ToList();
            return _parent.GetLinearStationsOrderedByDirection(Direction);
        }

        /// <inheritdoc />
        public ArrDep AddArrDep(Station sta, int route)
        {
            int idx;
            if (_parent.Type == TimetableType.Linear)
            {
                if (route != Timetable.LINEAR_ROUTE_ID)
                    throw new TimetableTypeNotSupportedException(TimetableType.Linear, "routes");
                var stas = _parent.GetLinearStationsOrderedByDirection(TrainDirection.ti);
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

        /// <inheritdoc />
        public ArrDep GetArrDep(Station sta)
        {
            if (TryGetArrDep(sta, out var arrDep))
                return arrDep;
            throw new Exception($"No ArrDep found for station {sta.SName}!");
        }

        /// <inheritdoc />
        public bool TryGetArrDep(Station sta, out ArrDep arrDep)
        {
            var tElems = InternalGetArrDeps();
            if (_parent.Type == TimetableType.Linear)
            {
                var stas = _parent.GetLinearStationsOrderedByDirection(TrainDirection.ti); // All arrdeps are sorted in line direction if linear
                var idx = stas.IndexOf(sta);
                arrDep = tElems[idx];
                return true;
            }
            arrDep = tElems.FirstOrDefault(t => t.StationId == sta.Id);
            return arrDep != null;
        }

        /// <inheritdoc />
        public Dictionary<Station, ArrDep> GetArrDepsUnsorted()
        {
            var ret = new Dictionary<Station, ArrDep>();
            var ardps = InternalGetArrDeps();
            foreach (var ardp in ardps)
            {
                var sta = _parent.Type == TimetableType.Network
                    ? _parent.GetStationById(ardp.StationId)
                    : _parent.GetLinearStationsOrderedByDirection(TrainDirection.ti)[Array.IndexOf(ardps, ardp)]; // All arrdeps are sorted in line direction if linear

                if (ret.ContainsKey(sta))
                    throw new Exception($"The path already contains the station \"{sta.SName}\"");

                ret.Add(sta, ardp);
            }
            return ret;
        }

        /// <inheritdoc />
        public void RemoveArrDep(Station sta)
        {
            var tElems = InternalGetArrDeps();
            ArrDep tElm;
            if (_parent.Type == TimetableType.Linear)
            {
                var stas = _parent.GetLinearStationsOrderedByDirection(TrainDirection.ti); // All arrdeps are sorted in line direction.
                var idx = stas.IndexOf(sta);
                tElm = tElems[idx];
            }
            else
                tElm = tElems.FirstOrDefault(t => t.StationId == sta.Id);

            if (tElm == null)
                return;

            Children.Remove(tElm.XMLEntity);
        }

        /// <inheritdoc />
        public void RemoveOrphanedTimes()
        {
            var stas = _parent.Type == TimetableType.Linear
                ? _parent.GetLinearStationsOrderedByDirection(TrainDirection.ti) // All arrdeps are sorted in line direction if linear...
                : GetPath(); // ...else, use full path.

            if (stas.Count == 0) // There is no remaining path, so nothing to clean up.
                return;

            var fs = stas.First();
            var ls = stas.Last();

            GetArrDep(fs).Arrival = default;
            GetArrDep(ls).Departure = default;
        }

        /// <summary>
        /// Returns all set arrdeps of this train.
        /// On Linear timetables, this includes also empty entries.
        /// LINEAR ENTRIES ARE SORTED IN TrainDirection.ti
        /// </summary>
        private ArrDep[] InternalGetArrDeps() => Children.Where(x => x.XName == "t").Select(x => new ArrDep(x, _parent)).ToArray();

        #endregion

        /// <inheritdoc />
        [XAttrName("fpl-tfz", IsFpleditElement = true)]
        public string Locomotive
        {
            get => GetAttribute("fpl-tfz", "");
            set => SetAttribute("fpl-tfz", value);
        }

        /// <inheritdoc />
        [XAttrName("fpl-mbr", IsFpleditElement = true)]
        public string Mbr
        {
            get => GetAttribute("fpl-mbr", "");
            set => SetAttribute("fpl-mbr", value);
        }

        /// <inheritdoc />
        [XAttrName("fpl-last", IsFpleditElement = true)]
        public string Last
        {
            get => GetAttribute("fpl-last", "");
            set => SetAttribute("fpl-last", value);
        }

        /// <inheritdoc />
        public TrainDirection Direction => (TrainDirection)Enum.Parse(typeof(TrainDirection), XMLEntity.XName);

        /// <inheritdoc />
        [XAttrName("cm")]
        public string Comment
        {
            get => GetAttribute("cm", "");
            set => SetAttribute("cm", value);
        }

        /// <inheritdoc />
        [XAttrName("d")]
        public Days Days
        {
            get => Days.Parse(GetAttribute("d", "1111111"));
            set => SetAttribute("d", value.ToBinString());
        }

        /// <summary>
        /// Creates a new train instance, without pre-existing data.
        /// </summary>
        /// <param name="dir">Direction of the newly created train. Only directions supported by the current Timetable type are allowed.</param>
        /// <param name="tt">The parent timetable instance.</param>
        /// <exception cref="TimetableTypeNotSupportedException">The given <see cref="TrainDirection" /> is not allowed in the current timetable type.</exception>#
        /// <remarks>The new <see cref="XMLEntity"/> will not be wird up in the XML tree.</remarks>
        public Train(TrainDirection dir, Timetable tt) : base(dir.ToString(), tt)
        {
            if (tt.Type == TimetableType.Network && dir != TrainDirection.tr)
                throw new TimetableTypeNotSupportedException(TimetableType.Network, "trains with direction");
            if (tt.Type == TimetableType.Linear && dir == TrainDirection.tr)
                throw new TimetableTypeNotSupportedException(TimetableType.Linear, "trains without direction");
        }

        /// <inheritdoc />
        public Train(XMLEntity en, Timetable tt) : base(en, tt)
        {
            if (Children.Count(x => x.XName == "t") > tt.Stations.Count)
                throw new Exception("Zu viele Fahrtzeiteneinträge im Vergleich zur Stationsanzahl!");
        }

        [DebuggerStepThrough]
        public override string ToString()
            => TName;

        /// <inheritdoc />
        // ReSharper disable once UnusedMember.Global
        public string GetLineName()
        {
            var path = GetPath();
            return path.First().SName + " - " + path.Last().SName;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Force.DeepCloner;

namespace FPLedit.Shared
{
    /// <inheritdoc cref="ITimetable" />
    [DebuggerDisplay("{" + nameof(TTName) + "}")]
    [XElmName("jTrainGraph_timetable")]
    [Templating.TemplateSafe]
    public sealed class Timetable : Entity, ITimetable
    {
        public const int LINEAR_ROUTE_ID = 0;
        public const int UNASSIGNED_ROUTE_ID = -1;
        public static TimetableVersion DefaultLinearVersion { get; set; } = TimetableVersion.JTG3_0;

        private readonly XMLEntity sElm, tElm, trElm;

        private int nextStaId = 0, nextRtId = 0, nextTraId = 0;
        
        private readonly Dictionary<int, Route> routeCache;

        #region XmlAttributes

        [XAttrName("version")]
        public TimetableVersion Version => (TimetableVersion) GetAttribute("version", 0);
        public TimetableType Type => Version == TimetableVersion.Extended_FPL ? TimetableType.Network : TimetableType.Linear;

        [XAttrName("name")]
        public string TTName
        {
            get => GetAttribute("name", "");
            set => SetAttribute("name", value);
        }

        [XAttrName("dTt")]
        public int DefaultPrePostTrackTime
        {
            get => GetAttribute("dTt", 10);
            set => SetAttribute("dTt", value.ToString());
        }

        #endregion

        private List<Station> stations;
        private readonly List<Train> trains;
        private readonly List<Transition> transitions;

        public IList<Station> Stations => stations.AsReadOnly();

        public IList<Train> Trains => trains.AsReadOnly();

        public IList<Transition> Transitions => transitions.AsReadOnly();

        /// <summary>
        /// Create a new new empty timetable file of the given timetable type.
        /// </summary>
        public Timetable(TimetableType type) : base("jTrainGraph_timetable", null) // Root without parent
        {
            stations = new List<Station>();
            trains = new List<Train>();
            transitions = new List<Transition>();

            SetAttribute("version", type == TimetableType.Network ? TimetableVersion.Extended_FPL.ToNumberString() : DefaultLinearVersion.ToNumberString()); // version="100" nicht kompatibel mit jTrainGraph
            sElm = new XMLEntity("stations");
            tElm = new XMLEntity("trains");
            trElm = new XMLEntity("transitions");
            Children.Add(sElm);
            Children.Add(tElm);
            
            routeCache = new Dictionary<int, Route>(); // Initialize empty route cache.
        }

        /// <summary>
        /// Create a timetable instance from the given <see cref="XMLEntity"/>.
        /// </summary>
        /// <exception cref="NotSupportedException">FPLedit does not support this timetable's <see cref="TimetableVersion"/>.</exception>
        internal Timetable(XMLEntity en) : base(en, null)
        {
            if (!Enum.IsDefined(typeof(TimetableVersion), Version))
                throw new NotSupportedException("Unbekannte Dateiversion. Nur mit jTrainGraph 2 oder 3.0 erstellte Dateien können geöffnet werden!");

            stations = new List<Station>();
            sElm = Children.FirstOrDefault(x => x.XName == "stations");
            if (sElm != null)
            {
                foreach (var c in sElm.Children.Where(x => x.XName == "sta")) // Filtert andere Elemente
                    stations.Add(new Station(c, this));
            }
            else
            {
                sElm = new XMLEntity("stations");
                Children.Add(sElm);
            }

            trains = new List<Train>();
            tElm = Children.FirstOrDefault(x => x.XName == "trains");
            if (sElm == null && tElm != null)
                throw new NotSupportedException("Kein <stations>-Element vorhanden, dafür aber <trains>!");

            if (tElm != null)
            {
                var directions = Enum.GetNames(typeof(TrainDirection));
                foreach (var c in tElm.Children.Where(x => directions.Contains(x.XName))) // Filtert andere Elemente
                    trains.Add(new Train(c, this));
            }
            else
            {
                tElm = new XMLEntity("trains");
                Children.Add(tElm);
            }

            transitions = new List<Transition>();
            trElm = Children.FirstOrDefault(x => x.XName == "transitions");
            if (trElm != null)
            {
                foreach (var c in trElm.Children.Where(x => x.XName == "tra")) // Filtert andere Elemente
                    transitions.Add(new Transition(c, this));
            }
            else
            {
                trElm = new XMLEntity("transitions");
                Children.Add(trElm);
            }

            // Höchste IDs ermitteln
            if (trains.Count > 0)
                nextTraId = trains.Max(s => s.Id);
            if (Type == TimetableType.Network)
            {
                nextStaId = stations.Max(s => s.Id);
                nextRtId = stations.SelectMany(s => s.Routes).DefaultIfEmpty().Max();
            }

            // Zügen ohne IDs diese neu zuweisen
            foreach (var train in trains)
            {
                if (train.Id == -1)
                    train.Id = ++nextTraId;
            }

            // Clean up invalid transitions
            var tids = trains.Select(t => t.Id).ToArray();
            foreach (var tra in Transitions.ToArray())
            {
                if (!tids.Contains(tra.First))
                    RemoveTransition(tra.First);
                else if (!tids.Contains(tra.Next))
                    RemoveTransition(tra.Next, false);
            }

            // Finally initialize route cache structure
            routeCache = _InternalGetRoutesUncached().ToDictionary(r => r.Index, r => r);
            
            if (routeCache == null)
                throw new Exception("RouteCache is null!");
            
            /*
             * !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
             * WARNING ROUTE CACHE IS NOW ACTIVE.
             * DON'T CHANGE ANYTHING LOWLEVEL WITHOUT MANUAL REBUILD!
             * !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
             */
        }

        public int NextTrainId() => ++nextTraId;

        #region Hilfsmethoden für Stationen

        /// <inheritdoc />
        /// <exception cref="ArgumentException">The station has already been registered and the timetable is a network timetable.</exception>
        /// <exception cref="TimetableTypeNotSupportedException">The timetable is linear but the given route index is not zero.</exception>
        public void AddStation(Station sta, int route)
        {
            sta._parent = this;

            if (Type == TimetableType.Network)
            {
                if (sta.GetAttribute<string>("fpl-id") != null)
                    throw new ArgumentException(nameof(sta) + "has already been registered!");
                sta.Id = ++nextStaId; // Neue Id an Station vergeben
            }

            stations.Add(sta);
            if (Type == TimetableType.Linear)
            {
                if (route != LINEAR_ROUTE_ID)
                    throw new TimetableTypeNotSupportedException(TimetableType.Linear, "routes");
                stations = GetLinearStationsOrderedByDirection(TrainDirection.ti);
                var idx = stations.IndexOf(sta); // Index vorläufig ermitteln

                // Es können ja noch andere Nodes in den Children sein.
                if (idx != 0)
                {
                    var staBefore = stations[idx - 1];
                    idx = sElm.Children.IndexOf(staBefore.XMLEntity) + 1;
                }
                else if (stations.Count > idx + 1)
                {
                    var staAfter = stations[idx + 1];
                    idx = sElm.Children.IndexOf(staAfter.XMLEntity); // Davor einfügen
                }
                sElm.Children.Insert(idx, sta.XMLEntity);
            }
            else
            {
                StationAddRoute(sta, route);
                sElm.Children.Add(sta.XMLEntity);
            }

            // Auch bei allen Zügen hinzufügen
            foreach (var t in Trains)
                t.AddArrDep(sta, route);
            
            RebuildRouteCache(route);
        }

        /// <inheritdoc />
        public void RemoveStation(Station sta)
        {
            foreach (var train in Trains)
                train.RemoveArrDep(sta);

            var needsCleanup = stations.First() == sta || stations.Last() == sta;
            var routes = sta.Routes;
            
            sta._parent = null;
            stations.Remove(sta);
            sElm.Children.Remove(sta.XMLEntity);
            
            // Rebuild route cache (before removing orphaned routes)
            foreach (var route in routes)
                RebuildRouteCache(route);

            if (!needsCleanup && Type == TimetableType.Linear)
                return;

            // Wenn Endstationen gelöscht werden könnten sonst korrupte Dateien entstehen!
            foreach (var train in Trains)
                train.RemoveOrphanedTimes();

            // Es können verwaiste Routen entstehen (requires route cache)
            if (Type == TimetableType.Network)
                RemoveOrphanedRoutes();
        }

        /// <inheritdoc />
        /// <exception cref="TimetableTypeNotSupportedException">Operation was applied to a linear timetable.</exception>
        public Station GetStationById(int id)
        {
            if (Type == TimetableType.Linear)
                throw new TimetableTypeNotSupportedException(TimetableType.Linear, "station ids");
            return stations.FirstOrDefault(s => s.Id == id);
        }
        
        #endregion
        
        #region Hilfsmethoden für Züge

        /// <inheritdoc />
        public void AddTrain(Train tra)
        {
            if (Trains.Contains(tra))
                return;
            
            tra.Id = ++nextTraId;
            tra._parent = this;
            trains.Add(tra);
            tElm.Children.Add(tra.XMLEntity);
        }

        /// <inheritdoc />
        public Train GetTrainById(int id)
            => trains.FirstOrDefault(t => t.Id == id);

        /// <inheritdoc />
        public void RemoveTrain(Train tra)
        {
            RemoveTransition(tra, false); // Remove "orphaned" transitions

            tra._parent = null;
            trains.Remove(tra);
            tElm.Children.Remove(tra.XMLEntity);
        }

        public void _InternalRenameAllTrainTracksAtStation(Station sta, string oldTrackName, string newTrackName)
        {
            foreach (var tra in Trains)
            {
                var ardps = tra.GetArrDepsUnsorted();
                if (!ardps.TryGetValue(sta, out var ardp))
                    return;
                
                if (ardp.ArrivalTrack == oldTrackName)
                    ardp.ArrivalTrack = newTrackName;
                if (ardp.DepartureTrack == oldTrackName)
                    ardp.DepartureTrack = newTrackName;
                
                foreach (var shunt in ardp.ShuntMoves)
                {
                    if (shunt.SourceTrack == oldTrackName)
                        shunt.SourceTrack = newTrackName;
                    if (shunt.TargetTrack == oldTrackName)
                        shunt.TargetTrack = newTrackName;
                }
            }
        }
        
        public void _InternalRemoveAllTrainTracksAtStation(Station sta, string oldTrackName)
        {
            foreach (var tra in Trains)
            {
                var ardps = tra.GetArrDepsUnsorted();
                if (!ardps.TryGetValue(sta, out var ardp))
                    return;
                
                if (ardp.ArrivalTrack == oldTrackName)
                    ardp.ArrivalTrack = "";
                if (ardp.DepartureTrack == oldTrackName)
                    ardp.DepartureTrack = "";
                
                var fixedShunts = ardp.ShuntMoves.ToArray(); // Copy of collection so that we can remove later on.
                foreach (var shunt in fixedShunts)
                {
                    if (shunt.SourceTrack == oldTrackName || shunt.TargetTrack == oldTrackName)
                        ardp.ShuntMoves.Remove(shunt);
                }
            }
        }
        
        public void _InternalSwapTrainOrder(Train t1, Train t2)
        {
            var idx = trains.IndexOf(t1);
            var xidx = tElm.Children.IndexOf(t1.XMLEntity);

            trains.Remove(t2);
            tElm.Children.Remove(t2.XMLEntity);

            trains.Insert(idx, t2);
            tElm.Children.Insert(xidx, t2.XMLEntity);
        }

        #endregion

        #region Hilfsmethoden für Routen
        
        private void RebuildRouteCache(int route)
        {
            if (routeCache == null)
                throw new Exception("RouteCache is null!");
            var stas = Stations.Where(s => s.Routes.Contains(route));
            routeCache[route] = new Route(route, stas);
        }
        
        private Route[] _InternalGetRoutesUncached()
        {
            if (Type == TimetableType.Network)
            {
                var routesJoin = Stations.SelectMany(s => s.Routes.Select(r => new {r, s}))
                    .GroupBy(o => o.r)
                    .Select(g => new Route(g.Key, g.Select(o => o.s))).ToArray();
                return routesJoin;
            }

            // TimetableType.Linear
            return new[] { new Route(LINEAR_ROUTE_ID, Stations) };
        }
        
        /// <inheritdoc />
        public void StationAddRoute(Station sta, int route)
        {
            if (sta._InternalAddRoute(route))
                RebuildRouteCache(route);
        }

        /// <inheritdoc />
        public void StationRemoveRoute(Station sta, int route)
        {
            if (sta._InternalRemoveRoute(route))
                RebuildRouteCache(route);
        }

        /// <inheritdoc />
        /// <exception cref="TimetableTypeNotSupportedException">Operation was applied to a linear timetable.</exception>
        /// <exception cref="ArgumentException"><paramref name="newStation"/> already had some routes.</exception>
        public int AddRoute(Station exisitingStartStation, Station newStation, float newStartPosition, float newPosition)
        {
            if (Type == TimetableType.Linear)
                throw new TimetableTypeNotSupportedException(TimetableType.Linear, "routes");
            if (newStation.Routes.Length > 0)
                throw new ArgumentException(nameof(newStation) + " has already some routes and is not plain.");
            
            var idx = ++nextRtId;

            StationAddRoute(exisitingStartStation, idx);
            AddStation(newStation, idx); // this will call StationAddRoute.

            exisitingStartStation.Positions.SetPosition(idx, newStartPosition);
            newStation.Positions.SetPosition(idx, newPosition);
            
            RebuildRouteCache(idx); // Create cache entry
            return idx;
        }

        /// <inheritdoc />
        public Route[] GetRoutes()
        {
            if (routeCache == null)
                throw new Exception("RouteCache is null!");
            return routeCache.Select(kvp => kvp.Value).ToArray();
        }

        /// <inheritdoc />
        /// <exception cref="TimetableTypeNotSupportedException">If called on a linear timetable.</exception>
        public Route GetRoute(int index)
        {
            if (Type == TimetableType.Linear && index == LINEAR_ROUTE_ID)
                return new Route(LINEAR_ROUTE_ID, Stations);
            if (Type == TimetableType.Linear && index != LINEAR_ROUTE_ID)
                throw new TimetableTypeNotSupportedException(TimetableType.Linear, "routes");
            
            if (routeCache == null)
                throw new Exception("RouteCache is null!");

            if (routeCache.TryGetValue(index, out var route))
                return route;
            RebuildRouteCache(index);
            
            if (routeCache.TryGetValue(index, out var route2))
                return route2;
            return new Route(index, Array.Empty<Station>());
        }

        /// <inheritdoc />
        /// <exception cref="TimetableTypeNotSupportedException">If called on a linear timetable.</exception>
        /// <exception cref="ArgumentException"><paramref name="station"/> already serves the Route <paramref name="route"/></exception>
        public void JoinRoutes(int route, Station station, float newKm)
        {
            if (Type == TimetableType.Linear)
                throw new TimetableTypeNotSupportedException(TimetableType.Linear, "routes");
            if (station.Routes.Contains(route))
                throw new ArgumentException(nameof(station) + " is already on route " + route);
            
            StationAddRoute(station, route);
            station.Positions.SetPosition(route, newKm);
            RebuildRouteCache(route);
        }

        /// <inheritdoc />
        public bool HasRouteCycles
        {
            get
            {
                if (Type == TimetableType.Linear)
                    throw new TimetableTypeNotSupportedException(TimetableType.Linear, "routes");

                return GetCyclicRoutes().Any();
            }
        }

        public IList<int> GetCyclicRoutes()
        {
            var junctions = Stations.Where(s => s.IsJunction); // All stations that are junction points.
            var rids = junctions.SelectMany(s => s.Routes).ToList(); // All routes participating in junctions (should be all).

            // Gets all routes, that have only one occurence in the juction-route graph above, so they have a "loose" end.
            int[] GetSingles() => rids
                .GroupBy(r => r)
                .Where(g => g.Count() == 1)
                .Select(g => g.Key)
                .ToArray();

            int[] singles = GetSingles();
            while (singles.Any())
            {
                for (int i = 0; i < singles.Length; i++)
                {
                    var s = singles[i];
                    // Find junction
                    var j = junctions.First(t => t.Routes.Contains(s)); // We only have one.
                    var r = j.Routes;
                    if (r.Length == 2) // Eliminate one instance of each route index, if only two remain at this station (which becomes a "loose" end now).
                        rids.Remove(r.First(t => t != s));
                    rids.Remove(s);
                }

                singles = GetSingles();
            }

            return rids;
        }

        /// <inheritdoc />
        public bool RouteConnectsDirectly(int routeToCheck, Station sta1, Station sta2)
        {
            var path = GetRoute(routeToCheck).Stations;
            var idx1 = path.IndexOf(sta1);
            var idx2 = path.IndexOf(sta2);
            if (idx1 == -1 || idx2 == -1)
                return false;
            return Math.Abs(idx1 - idx2) == 1;
        }

        /// <inheritdoc />
        public int GetDirectlyConnectingRoute(Station sta1, Station sta2)
            => sta1.Routes.Intersect(sta2.Routes).DefaultIfEmpty(-1)
                .FirstOrDefault(r => RouteConnectsDirectly(r, sta1, sta2));

        private void RemoveOrphanedRoutes()
        {
            if (Type != TimetableType.Network)
                return;

            foreach (var route in GetRoutes())
            {
                if (route.Stations.Count >= 2)
                    continue;

                foreach (var rsta in route.Stations)
                    StationRemoveRoute(rsta, route.Index);
            }
        }
        
        #endregion

        #region Hilfsmethoden für Umläufe

        /// <summary>
        /// Private helper function to create a new transition. Only exposed via <see cref="SetTransition"/>.
        /// </summary>
        private void AddTransition(Train first, Train next)
        {
            if (next == null) return;
            var transition = new Transition(this)
            {
                First = first.Id,
                Next = next.Id
            };
            trElm.Children.Add(transition.XMLEntity);
            transitions.Add(transition);
        }

        /// <inheritdoc />
        public void SetTransition(Train first, Train newNext)
        {
            var trans = transitions.Where(t => t.First == first.Id).ToArray();

            if (!trans.Any() && newNext != null)
                AddTransition(first, newNext);
            else if (trans.Length > 1)
                throw new Exception("Mehr als eine Transition mit angegebenem ersten Zug gefunden!");
            else if (trans.Length == 1)
            {
                if (newNext != null)
                    trans.First().Next = newNext.Id;
                else
                    RemoveTransition(first);
            }
        }

        /// <inheritdoc />
        public Train GetTransition(Train first)
        {
            return first != null ? GetTransition(first.Id) : null;
        } 

        /// <inheritdoc />
        public Train GetTransition(int firstTrainId)
        {
            if (firstTrainId == -1)
                return null;
            
            var trans = transitions.Where(t => t.First == firstTrainId).ToArray();

            if (!trans.Any())
                return null;
            if (trans.Length > 1)
                throw new Exception("Mehr als eine Transition mit angegebenem ersten Zug gefunden!");

            return GetTrainById(trans.First().Next);
        }

        /// <inheritdoc />
        public IEnumerable<Train> GetFollowingTransitions(Train first)
        {
            var tra = first;
            while ((tra = GetTransition(tra)) != null && tra != first)
                yield return tra;
        }

        /// <inheritdoc />
        public void RemoveTransition(Train first, bool onlyAsFirst = true) => RemoveTransition(first.Id, onlyAsFirst);

        /// <inheritdoc />
        public void RemoveTransition(int firstTrainId, bool onlyAsFirst = true)
        {
            var trans = transitions.Where(t => t.First == firstTrainId || (!onlyAsFirst && t.Next == firstTrainId)).ToArray();
            foreach (var transition in trans)
                trElm.Children.Remove(transition.XMLEntity);
            transitions.RemoveAll(t => trans.Contains(t));
        }

        /// <inheritdoc />
        public bool HasTransition(Train tra, bool onlyAsFirst = true)
            => transitions.Any(t => t.First == tra.Id || (!onlyAsFirst && t.Next == tra.Id));

        #endregion
        
        #region Uneindeutige Routen

        /// <inheritdoc />
        public bool WouldProduceAmbiguousRoute(Station toDelete)
        {
            var routesToCheck = toDelete.Routes;
            foreach (var route in routesToCheck)
            {
                var routeStations = GetRoute(route);
                var junctions = routeStations.GetSurroundingStations(toDelete, 1);
                if (junctions.Length != 3)
                    continue; // we are ath the edge of a route.
                var routes = junctions.SelectMany(j => j.Routes).Distinct();
                var intersect = routes.Intersect(routesToCheck);
  
                var routeChandidates = routes.Except(intersect);
                foreach (var candidate in routeChandidates)
                {
                    if (RouteConnectsDirectly(candidate, junctions[0], junctions[2]))
                        return true;
                }
            }

            return false;
        }
        
        #endregion
        
        /// <inheritdoc />
        public Timetable Clone() => this.DeepClone();

        [DebuggerStepThrough]
        public override string ToString()
            => string.Join(" | ", GetRoutes().Select(r => r.GetRouteName()));
        
        #region Legacy linear APIs
        
        /// <inheritdoc />
        /// <exception cref="TimetableTypeNotSupportedException">Operation was applied to a network timetable, or with a network type direction (tr).</exception>
        [Obsolete("Use route-based approach instead.")]
        public List<Station> GetLinearStationsOrderedByDirection(TrainDirection direction)
        {
            if (Type == TimetableType.Network)
                throw new TimetableTypeNotSupportedException(TimetableType.Network, "direction");
            if (direction == TrainDirection.tr)
                throw new TimetableTypeNotSupportedException(TimetableType.Network, "direction value tr");
            
            return (direction == TrainDirection.ta
                ? GetRoute(LINEAR_ROUTE_ID).Stations.Reverse()
                : GetRoute(LINEAR_ROUTE_ID).Stations).ToList();
        }

        /// <inheritdoc />
        /// <exception cref="TimetableTypeNotSupportedException">Operation was applied to a network timetable.</exception>
        [Obsolete("Use route-based approach instead.")]
        public string GetLinearLineName(TrainDirection direction)
        {
            if (Type == TimetableType.Network)
                throw new TimetableTypeNotSupportedException(TimetableType.Network, "direction");
            if (direction == TrainDirection.tr)
                throw new TimetableTypeNotSupportedException(TimetableType.Network, "direction value tr");

            var stas = GetLinearStationsOrderedByDirection(direction);

            if (!stas.Any())
                return "";
            
            return stas.First().SName + " - " + stas.Last().SName;
        }
        
        #endregion
    }
}
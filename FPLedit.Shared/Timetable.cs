using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Force.DeepCloner.Helpers;

namespace FPLedit.Shared;

/// <inheritdoc cref="ITimetable" />
[DebuggerDisplay("{" + nameof(TTName) + "}")]
[XElmName("jTrainGraph_timetable")]
[Templating.TemplateSafe]
public sealed class Timetable : Entity, ITimetable
{
    public const int LINEAR_ROUTE_ID = 0;
    public const int UNASSIGNED_ROUTE_ID = -1;

    public const TimetableVersion PRESET_LINEAR_VERSION = TimetableVersion.JTG3_3;
    public const TimetableVersion PRESET_NETWORK_VERSION = TimetableVersion.Extended_FPL2;
    public static TimetableVersion DefaultLinearVersion { get; set; } = PRESET_LINEAR_VERSION;
    public static TimetableVersion DefaultNetworkVersion { get; set; } = PRESET_NETWORK_VERSION;

    private readonly XMLEntity sElm, tElm, trElm, vElm;

    private int nextStaId, nextRtId, nextTraId, nextVehId;

    private readonly Dictionary<int, Route> routeCache;

    #region XmlAttributes

    private TimetableType? typeCache;

    [XAttrName("version")]
    public TimetableVersion Version => (TimetableVersion) GetAttribute("version", 0);

    public TimetableType Type => typeCache ??= Version.GetVersionCompat().Type;

    public void SetVersion(TimetableVersion version)
    {
        if (version.GetVersionCompat().Compatibility != TtVersionCompatType.ReadWrite)
            throw new NotSupportedException(T._("Nicht schreibbare Dateiversion."));
        SetAttribute("version", version.ToNumberString());
        typeCache = null;
    }

    /// <inheritdoc />
    [XAttrName("name")]
    public string TTName
    {
        get => GetAttribute("name", "");
        set => SetAttribute("name", value);
    }

    /// <inheritdoc />
    [XAttrName("dTt")]
    public TimeEntry DefaultPrePostTrackTime
    {
        get => TimeEntry.Parse(GetAttribute("dTt", "00:10"));
        set => SetAttribute("dTt", value.ToString());
    }

    #endregion

    private List<Station> stations;
    private readonly List<ITrain> trains;
    private readonly List<Transition> transitions;
    private readonly List<Vehicle> vehicles;

    public IList<Station> Stations => stations.AsReadOnly();

    public IList<ITrain> Trains => trains.AsReadOnly();

    public IList<Transition> Transitions => transitions.AsReadOnly();

    /// <inheritdoc />
    public event EventHandler? TrainsXmlCollectionChanged;

    /// <summary>
    /// Create a new new empty timetable file of the given timetable type.
    /// </summary>
    public Timetable(TimetableType type) : base("jTrainGraph_timetable", null) // Root without parent
    {
        stations = new List<Station>();
        trains = new List<ITrain>();
        transitions = new List<Transition>();
        vehicles = new List<Vehicle>();

        SetAttribute("version", (type == TimetableType.Network ? DefaultNetworkVersion : DefaultLinearVersion).ToNumberString());
        sElm = new XMLEntity("stations");
        tElm = new XMLEntity("trains");
        trElm = new XMLEntity("transitions");
        vElm = new XMLEntity("vehicles");
        Children.Add(sElm);
        Children.Add(tElm);
        Children.Add(trElm);
        Children.Add(vElm);

        tElm.ChildrenChangedDirect += OnTrainsChanged;
            
        routeCache = new Dictionary<int, Route>(); // Initialize empty route cache.
    }

    private void OnTrainsChanged(object? sender, EventArgs e)
    {
        TrainsXmlCollectionChanged?.Invoke(sender, e);
    }

    /// <summary>
    /// Create a timetable instance from the given <see cref="XMLEntity"/>.
    /// </summary>
    /// <exception cref="NotSupportedException">FPLedit does not support this timetable's <see cref="TimetableVersion"/> or it is not writable.</exception>
    public Timetable(XMLEntity en) : base(en, null)
    {
        if (!Enum.IsDefined(typeof(TimetableVersion), Version))
            throw new NotSupportedException(T._("Unbekannte Dateiversion."));

        if (Version.GetVersionCompat().Compatibility != TtVersionCompatType.ReadWrite)
            throw new NotSupportedException(T._("Nicht schreibbare Dateiversion.")); // Do not create any properties as we cannot read this format.

        stations = new List<Station>();
        var tmpSElm = Children.SingleOrDefault(x => x.XName == "stations");
        if (tmpSElm != null)
        {
            sElm = tmpSElm;
            foreach (var c in sElm.Children.Where(x => x.XName == "sta")) // Filtert andere Elemente
                stations.Add(new Station(c, this));
        }
        else
        {
            sElm = new XMLEntity("stations");
            Children.Add(sElm);
        }

        trains = new List<ITrain>();
        var tmpTElm = Children.SingleOrDefault(x => x.XName == "trains");
        if (tmpSElm == null && tmpTElm != null)
            throw new NotSupportedException("No <stations> element exists, but <trains>!");

        if (tmpTElm != null)
        {
            tElm = tmpTElm;
                
            var directions = Enum.GetNames(typeof(TrainDirection));
            var trainElements = tElm.Children.Where(x => directions.Contains(x.XName)).ToArray();
            var trainLinkElements = new Dictionary<(TrainDirection, int), TrainLink>();
            var linkedTrainsMap = new Stack<(int, XMLEntity)>();

            var counters = new Dictionary<string, int>(3)
            {
                [TrainDirection.ta.ToString()] = 0,
                [TrainDirection.ti.ToString()] = 0,
                [TrainDirection.tr.ToString()] = 0,
            };
            foreach (var c in trainElements) // Filtert andere Elemente
            {
                if (!c.GetAttribute<bool>("islink"))
                {
                    var train = new Train(c, this);
                    trains.Add(train);
                    foreach (var link in train.TrainLinks)
                    foreach (var linkIndex in link.TrainIndices)
                        trainLinkElements.Add((link.ParentTrain.Direction, linkIndex), link);
                }
                else
                    linkedTrainsMap.Push((counters[c.XName], c));

                counters[c.XName]++;
            }

            // Correction to compensate deleted orphaned trains.
            var indexCorrection = new Dictionary<string, int>(3)
            {
                [TrainDirection.ta.ToString()] = 0,
                [TrainDirection.ti.ToString()] = 0,
                [TrainDirection.tr.ToString()] = 0,
            };
            var trainsToAdd = new List<(int, LinkedTrain)>();
            // Match linked trains
            while (linkedTrainsMap.Any())
            {
                var (idx, xmlEntity) = linkedTrainsMap.Pop();
                var direction = (TrainDirection)Enum.Parse(typeof(TrainDirection), xmlEntity.XName);
                if (trainLinkElements.TryGetValue((direction, idx), out var linkElement))
                {
                    var counting = Array.IndexOf(linkElement.TrainIndices, idx);
                    var linkedTrain = new LinkedTrain(linkElement, counting, xmlEntity);
                    trainsToAdd.Add((idx + indexCorrection[xmlEntity.XName], linkedTrain));
                    linkElement._InternalInjectLinkedTrain(linkedTrain, counting);
                }
                else
                {
                    // This XML node has no corresponding parent train/link element, so we can delete it.
                    tElm.Children.Remove(xmlEntity);
                    indexCorrection[xmlEntity.XName]--;
                }
            }

            foreach (var (idx, lt) in trainsToAdd.OrderBy(e => e.Item1))
                trains.Insert(idx, lt);
        }
        else
        {
            tElm = new XMLEntity("trains");
            Children.Add(tElm);
        }

        tElm.ChildrenChangedDirect += OnTrainsChanged;

        transitions = new List<Transition>();
        var tmpTrElm = Children.SingleOrDefault(x => x.XName == "transitions");
        if (tmpTrElm != null)
        {
            trElm = tmpTrElm;
            foreach (var c in tmpTrElm.Children.Where(x => x.XName == "tra")) // Filtert andere Elemente
                transitions.Add(new Transition(c, this));
        }
        else
        {
            trElm = new XMLEntity("transitions");
            Children.Add(trElm);
        }

        vehicles = new List<Vehicle>();
        var tmpVElm = Children.SingleOrDefault(x => x.XName == "vehicles");
        if (tmpVElm != null)
        {
            vElm = tmpVElm;
            foreach (var c in trElm.Children.Where(x => x.XName == "veh")) // Filtert andere Elemente
                vehicles.Add(new Vehicle(c, this));
        }
        else
        {
            vElm = new XMLEntity("vehicles");
            Children.Add(vElm);
        }

        if (vehicles.Any())
            nextVehId = vehicles.Max(v => v.Id);

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
            if (train is IWritableTrain wt && wt.Id == -1)
                wt.Id = AssignNextTrainId();
        }

        // Clean up invalid transitions
        var tids = trains.Select(t => t.QualifiedId).ToArray();
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

    public int AssignNextTrainId() => ++nextTraId;
    public int AssignNextRouteId() => ++nextRtId;

    #region Hilfsmethoden für Stationen

    /// <inheritdoc />
    /// <exception cref="ArgumentException">The station has already been registered and the timetable is a network timetable.</exception>
    /// <exception cref="TimetableTypeNotSupportedException">The timetable is linear but the given route index is not zero.</exception>
    public void AddStation(Station sta, int route)
    {
        sta.ParentTimetable = this;

        if (Type == TimetableType.Network)
        {
            if (sta.GetAttribute<string>("fpl-id") != null)
                throw new ArgumentException(nameof(sta) + " has already been registered!");
            sta.Id = ++nextStaId; // Assign new id to the station.
        }

        stations.Add(sta);
        RebuildRouteCache(route);
            
        if (Type == TimetableType.Linear)
        {
            if (route != LINEAR_ROUTE_ID)
                throw new TimetableTypeNotSupportedException(TimetableType.Linear, "routes");
            stations = GetRoute(LINEAR_ROUTE_ID).Stations.ToList(); // Replace collection with an ordered one.
            var idx = stations.IndexOf(sta); // Get temporary index.
            var collectionIndex = idx;

            // Get xml entity index of the previous/next station, to handle other xml entity types.
            if (idx > 0)
            {
                var staBefore = stations[idx - 1];
                idx = sElm.Children.IndexOf(staBefore.XMLEntity) + 1;
            }
            else if (idx == 0 && stations.Count > 1)
            {
                var staAfter = stations[1];
                idx = sElm.Children.IndexOf(staAfter.XMLEntity); // Insert before.
            }
            else if (stations.Count > 1)
                throw new Exception("Invalid negative index encountered!");
            sElm.Children.Insert(idx, sta.XMLEntity);

            // Increment index of all following stations that are referenced in transitions.
            foreach (var transition in transitions)
            {
                if (int.TryParse(transition.StationId, out var numericStationId) && numericStationId > collectionIndex)
                    transition.StationId = (numericStationId + 1).ToString();
            }
        }
        else
        {
            StationAddRoute(sta, route);
            sElm.Children.Add(sta.XMLEntity);
        }

        // Add station to all trains.
        foreach (var t in Trains)
            if (t is IWritableTrain wt)
                wt.AddArrDep(sta, route);
            
        RebuildRouteCache(route);
    }

    /// <inheritdoc />
    public void RemoveStation(Station sta)
    {
        foreach (var train in Trains)
            if (train is IWritableTrain wt)
                wt.RemoveArrDep(sta);

        var needsCleanup = stations.First() == sta || stations.Last() == sta;
        var routes = sta.Routes;

        // Clean up all transitions which were only valid at this sttaion.
        var id = Type == TimetableType.Linear ? GetRoute(Timetable.LINEAR_ROUTE_ID).IndexOf(sta) : sta.Id;
        foreach (var transition in transitions)
        {
            if (int.TryParse(transition.StationId, out var numericStationId) && numericStationId == id)
                transition.StationId = Transition.LAST_STATION; // Reset to default.
        }
            
        sta.ParentTimetable = null;
        stations.Remove(sta);
        sElm.Children.Remove(sta.XMLEntity);
            
        // Rebuild route cache (before removing orphaned routes)
        foreach (var route in routes)
            RebuildRouteCache(route);

        if (!needsCleanup && Type == TimetableType.Linear)
            return;

        // Remove orphaned time entries at new last/first stations.
        foreach (var train in Trains)
            if (train is IWritableTrain wt)
                wt.RemoveOrphanedTimes();

        // Remove orphaned routes, if applicable (requires route cache).
        if (Type == TimetableType.Network)
            RemoveOrphanedRoutes();
    }

    /// <inheritdoc />
    /// <exception cref="TimetableTypeNotSupportedException">Operation was applied to a linear timetable.</exception>
    public Station? GetStationById(int id)
    {
        if (Type == TimetableType.Linear)
            throw new TimetableTypeNotSupportedException(TimetableType.Linear, "station ids");
        return stations.FirstOrDefault(s => s.Id == id);
    }
        
    #endregion
        
    #region Hilfsmethoden für Züge

    /// <inheritdoc />
    public void AddTrain(ITrain tra)
    {
        if (Trains.Contains(tra))
            return;
            
        if (tra is IWritableTrain wt)
            wt.Id = AssignNextTrainId();
        tra.ParentTimetable = this;
        trains.Add(tra);
        tElm.Children.Add(tra.XMLEntity);
    }

    /// <inheritdoc />
    public ITrain? GetTrainById(int id)
        => trains.FirstOrDefault(t => t.Id == id && !t.IsLink);
        
    /// <inheritdoc />
    public ITrain? GetTrainByQualifiedId(string qid)
        => trains.FirstOrDefault(t => t.QualifiedId == qid);

    /// <inheritdoc />
    public void RemoveTrain(ITrain tra)
    {
        RemoveTransition(tra, false); // Remove "orphaned" transitions
        RemoveOrphanedVehicleStarts(tra);

        tra.ParentTimetable = null;
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
        
    public void _InternalSwapTrainOrder(ITrain t1, ITrain t2)
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
        
    internal void RebuildRouteCache(int route)
    {
        if (routeCache == null)
            throw new Exception("RouteCache is null!");
        var stas = Stations.Where(s => s.Routes.Contains(route));
        if (!stas.Any())
            routeCache.Remove(route); // Remove non-existing routes from cache.
        else
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

        var idx = AssignNextRouteId();

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
        return routeCache.Values.ToArray();
    }

    /// <inheritdoc />
    /// <exception cref="TimetableTypeNotSupportedException">If called on a linear timetable.</exception>
    public Route GetRoute(int index)
    {
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
    public bool JoinRoutes(int route, Station station, float newKm)
    {
        if (Type == TimetableType.Linear)
            throw new TimetableTypeNotSupportedException(TimetableType.Linear, "routes");
        if (station.Routes.Contains(route))
            throw new ArgumentException(nameof(station) + " is already on route " + route);

        // Try-add join route.
        StationAddRoute(station, route);
        station.Positions.SetPosition(route, newKm);
        RebuildRouteCache(route);
            
        // All stations that are junction points.
        var maybeAffectedRoutes = station.Routes.Concat(new[] { route }).ToArray();
        var junctions = Stations.Where(s => s.IsJunction && s.Routes.Intersect(maybeAffectedRoutes).Any()).Concat(new []{station}).Distinct().ToArray();
        var hasAmbiguousRoutes = CheckAmbiguousRoutesInternal(junctions);

        // If we have ambiguous routes after this change, revert.
        if (hasAmbiguousRoutes)
        {
            station.Positions.RemovePosition(route);
            StationRemoveRoute(station, route);
            RebuildRouteCache(route);
            return false;
        }

        return true;
    }

    /// <inheritdoc />
    /// <exception cref="TimetableTypeNotSupportedException">If called on a linear timetable.</exception>
    public (bool success, string? failReason, bool? isSafeFailure, int? routeToReload) BreakRouteUnsafe(Station station1, Station station2)
    {
        if (Type == TimetableType.Linear)
            throw new TimetableTypeNotSupportedException(TimetableType.Linear, "routes");

        if (GetDirectlyConnectingRoute(station1, station2) == UNASSIGNED_ROUTE_ID)
            return (false, T._("Die beiden ausgewählten Stationen sind nicht benachbart!"), true, null);

        var routes = station1.Routes.Where(r => station2.Routes.Contains(r)).ToArray();
        if (routes.Length != 1)
            return (false, T._("Zwei benachbarte Stationen können nicht mehr als eine/keine Route gemeinsam haben! Zusammengefallene Routen sind vorhanden und werden nicht unterstützt."), true, null);
        var route = routes[0];
        var rt = GetRoute(route);
        if (!rt.Exists)
            throw new Exception(nameof(BreakRouteUnsafe) + ": route does not exist");

        // Helper function to test for the station graph being disconnected after a "succesful" route break.
        (bool success, string? failReason, bool? isSafeFailure, int? origRoute) CheckConnectedAndReturn()
        {
            if (CheckStationGraphInternal(checkDisconnected: true).hasDisconnected)
                return (false, T._("Das Streckennetz zerfällt nach der Auftrennung der Strecke in zwei Teile."), false, route);
            return (true, null, null, route);
        }

        // Breaking a line is not possible when trains travel over the segment.
        foreach (var train in Trains)
        {
            var path = train.GetPath();
            if (path.Contains(station1) && path.Contains(station2))
                return (false, T._("Mindestens ein Zug befährt den Streckenabschnitt!"), true, null);
        }

        if (rt.Stations.Count < 2)
            throw new Exception(nameof(BreakRouteUnsafe) + ": route with less than two stations?!");

        // Trivial case: if the route only has two stations.
        if (rt.Stations.Count == 2)
        {
            if (station1.Routes.Length <= 1 || station2.Routes.Length <= 1)
                return (false, T._("Einzelne Stationen ohne Strecken können nicht existieren. Hinweis: Das Löschen der vorletzten Station einer Strecke entfernt auch Routen-Reste!"), true, null); // Otherwise we would have an "orphaned" station.
            StationRemoveRoute(station1, route);
            StationRemoveRoute(station2, route);
            return CheckConnectedAndReturn();
        }

        // Trivial case: If the station is the last/first of the line, just remove the route and be good.
        if (rt.Stations.First() == station1 || rt.Stations.Last() == station1)
        {
            if (station1.Routes.Length <= 1)
                return (false, T._("Einzelne Stationen ohne Strecken können nicht existieren. Hinweis: Erste/Letzte Stationen einer Strecke können einfach gelöscht werden!"), true, null); // Otherwise we would have an "orphaned" station.
            StationRemoveRoute(station1, route);
            return CheckConnectedAndReturn();
        }
        if (rt.Stations.First() == station2 || rt.Stations.Last() == station2)
        {
            if (station2.Routes.Length <= 1)
                return (false, T._("Einzelne Stationen ohne Strecken können nicht existieren. Hinweis: Erste/Letzte Stationen einer Strecke können einfach gelöscht werden!"), true, null); // Otherwise we would have an "orphaned" station.
            StationRemoveRoute(station2, route);
            return CheckConnectedAndReturn();
        }

        // Now all the trivial cases have been dealt with. We really need a new route id, as the split is
        // somewhere in the middle of the line.
        var newRtId = AssignNextRouteId();

        var newRouteStations = rt.Stations.SkipWhile(s => s != station1).Skip(1).ToList();
        if (newRouteStations.Count == 0 || newRouteStations.First() != station2)
            throw new Exception(nameof(BreakRouteUnsafe) + ": split is more than one station?");

        foreach (var newRtStation in newRouteStations)
        {
            if (!newRtStation._InternalReplaceRoute(route, newRtId))
                throw new Exception(nameof(BreakRouteUnsafe) + ": trying to move station now on route?");
        }

        // Build caches for both routes.
        RebuildRouteCache(route);
        RebuildRouteCache(newRtId);

        return CheckConnectedAndReturn();
    }

    /// <summary>
    /// Internal function to check for ambiguous routes at the given junctions.
    /// </summary>
    /// <param name="junctions">Collection of junction points.</param>
    /// <returns>Whether ambiguous routes exist.</returns>
    public bool CheckAmbiguousRoutesInternal(Station[] junctions)
    {
        var hasAmbiguousRoutes = false;

        for (int i = 0; i < junctions.Length - 1; i++)
        {
            for (int j = i + 1; j < junctions.Length; j++)
            {
                hasAmbiguousRoutes |= (junctions[i].Routes.Intersect(junctions[j].Routes).DefaultIfEmpty(-1)
                    .Count(r => RouteConnectsDirectly(r, junctions[i], junctions[j])) > 1);
            }
        }

        return hasAmbiguousRoutes;
    }

    /// <inheritdoc />
    public bool HasRouteCycles
    {
        get
        {
            if (Type == TimetableType.Linear)
                throw new TimetableTypeNotSupportedException(TimetableType.Linear, "routes");

            var graphCheck = CheckStationGraphInternal();
            return graphCheck.hasCycles;
        }
    }

    private (bool hasCycles, bool hasDisconnected) CheckStationGraphInternal(bool checkDisconnected = false)
    {
        if (!Stations.Any())
            return (false, false);

        const int unvisited = 0, visiting = 1, finalized = 2;
        var marking = new Dictionary<Station, int>();
        var predecessor = new Dictionary<Station, Station?>();
        foreach (var s in Stations)
        {
            marking[s] = unvisited;
            predecessor[s] = null;
        }

        var hasCycles = false;

        // Perform a DFS on the station graph. Each node (station) can either be unvisited, currently in progess
        // visiting or finalized.
        // Visit the full graph (starting with one arbitrary station), to also look for disconnected parts of the
        // graph (which we do not support).
        void VisitStation(Station u)
        {
            marking[u] = visiting;
            // Get all adjacent stations of the current station u.
            var adjacent = new HashSet<Station>();
            foreach (var r in u.Routes)
            {
                foreach (var a in routeCache[r].GetSurroundingStations(u, 1))
                    adjacent.Add(a);
            }
            adjacent.Remove(u); // GetSurroundingStations also returns the current station, so remove it again.

            foreach (var a in adjacent)
            {
                if (marking[a] == visiting && predecessor[u] != a)
                    hasCycles = true;
                if (marking[a] == unvisited)
                {
                    predecessor[a] = u;

                    RuntimeHelpers.EnsureSufficientExecutionStack();
                    VisitStation(a);
                }
            }

            marking[u] = finalized;
        }

        VisitStation(Stations.First());

        // If we still have unvisited stations after performing the DFS, we have disconnected subgraphs :-(
        var hasDisconnected = false;
        if (checkDisconnected)
            hasDisconnected = marking.Any(m => m.Value == unvisited);

        return (hasCycles, hasDisconnected);
    }

    public IList<int> GetCyclicRoutes()
    {
        var junctions = Stations.Where(s => s.IsJunction).ToArray(); // All stations that are junction points.
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
        => sta1.Routes.Intersect(sta2.Routes)
            .FirstOrDefault(r => RouteConnectsDirectly(r, sta1, sta2), UNASSIGNED_ROUTE_ID);

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

    public IList<TransitionEntry> GetEditableTransitions(ITrain first)
    {
        if (first.IsLink)
            return new List<TransitionEntry>();

        return GetEditableTransitionsInternal(first)
            .Select(t => new { t, n = GetTrainByQualifiedId(t.Next) })
            .Where(t => t.n != null)
            .Select(t => new TransitionEntry(t.n!, t.t.Days, t.t.StationId != Transition.LAST_STATION && int.TryParse(t.t.StationId, out var sId) ? GetStationById(sId) : null))
            .ToList();
    }
        
    private IEnumerable<Transition> GetEditableTransitionsInternal(ITrain first)
    {
        if (first.IsLink)
            return Array.Empty<Transition>();

        return transitions
            .Where(t => t.First == first.QualifiedId)
            .Where(t => t.GetAttribute<string>("vc") == null);
    }

    /// <inheritdoc />
    public void SetTransitions(ITrain first, IEnumerable<TransitionEntry> trans)
    {
        var editable = GetEditableTransitionsInternal(first).ToArray();
        RemoveTransitionsInternal(editable); // Clean up.

        foreach (var tran in trans)
        {
            var t = new Transition(this)
            {
                First = first.QualifiedId,
                Next = tran.NextTrain.QualifiedId,
                Days = tran.Days,
                StationId = tran.Station != null ? "" : Transition.LAST_STATION,
            };
            trElm.Children.Add(t.XMLEntity);
            transitions.Add(t);
        }
    }

    /// <inheritdoc />
    public ITrain? GetTransition(ITrain? first, Days? daysFilter = null, Station? stationFilter = null)
    {
        if (first == null)
            return null;
        if (!first.IsLink)
            return GetTransitionUnLinked(first.QualifiedId, daysFilter, stationFilter);

        // Unroll linked trains.
        var linkedTrain = (LinkedTrain) first;
        var link = linkedTrain.Link;
        if (!link.CopyTransitions)
            return null;
        var parentNext = GetTransition(link.ParentTrain, daysFilter, stationFilter);
        if (parentNext == null)
            return null;
        if ((parentNext is IWritableTrain wt) && link.TrainLinkIndex < wt.TrainLinks.Length)
        {
            var nextLink = wt.TrainLinks[link.TrainLinkIndex];
            if (nextLink == null! || linkedTrain.LinkCountingIndex >= nextLink.TrainCount)
                return null;
            return nextLink.LinkedTrains[linkedTrain.LinkCountingIndex];
        }
        if (parentNext.IsLink && parentNext is LinkedTrain lt)
        {
            if (linkedTrain.LinkCountingIndex + lt.LinkCountingIndex + 1 >= lt.Link.TrainCount)
                return null;
            return lt.Link.LinkedTrains[linkedTrain.LinkCountingIndex + lt.LinkCountingIndex + 1];
        }

        return null;
    }

    internal ITrain? GetTransitionUnLinked(string firstQualifiedTrainId, Days? daysFilter = null, Station? stationFilter = null)
    {
        if (firstQualifiedTrainId == "-1" || string.IsNullOrWhiteSpace(firstQualifiedTrainId))
            return null;

        bool Filter(Transition tra)
        {
            var result = tra.First == firstQualifiedTrainId;
            result &= daysFilter == null || tra.Days.IsIntersecting(daysFilter.Value);
            result &= stationFilter == null || tra.IsTransitionValidAt(stationFilter);
            result &= tra.GetAttribute<string>("vc") == null;

            return result;
        }
            
        var trans = transitions.Where(Filter).ToArray();

        if (!trans.Any())
            return null;
        if (trans.Length > 1)
            throw new AmbiguousTransitionException("Found more than one transition matching the given criteria!");

        return GetTrainByQualifiedId(trans.First().Next);
    }

    /// <inheritdoc />
    public void RemoveTransition(ITrain first, bool onlyAsFirst = true) => RemoveTransition(first.QualifiedId, onlyAsFirst);

    /// <inheritdoc />
    public void RemoveTransition(string firstTrainId, bool onlyAsFirst = true)
    {
        var trans = transitions.Where(t => t.First == firstTrainId || (!onlyAsFirst && t.Next == firstTrainId)).ToArray();
        RemoveTransitionsInternal(trans);
    }
        
    private void RemoveTransitionsInternal(Transition[] trans)
    {
        foreach (var transition in trans)
            trElm.Children.Remove(transition.XMLEntity);
        transitions.RemoveAll(trans.Contains);
    }

    /// <inheritdoc />
    public bool HasTransition(ITrain tra, bool onlyAsFirst = true)
        => transitions.Any(t => t.First == tra.QualifiedId || (!onlyAsFirst && t.Next == tra.QualifiedId));

    #endregion
        
    #region Fahrzeuge und Umläufe
        
    private void RemoveOrphanedVehicleStarts(ITrain tra)
    {
        foreach (var veh in vehicles)
            veh.RemoveStartTrains(tra);
    }
        
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
            var routes = junctions.SelectMany(j => j.Routes).Distinct().ToArray();
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
        
    #region Hilfsmethoden für Links

    /// <inheritdoc />
    public void RemoveLink(TrainLink link)
    {
        foreach (var train in link.LinkedTrains)
            RemoveTrain(train);

        string DoTraReplace(string qid)
        {
            if (!qid.Contains(';'))
                return qid;
            var parts = qid.Split(';');
            if (parts[0] != link.ParentTrain.QualifiedId)
                return qid;
            if (int.Parse(parts[1]) > link.TrainLinkIndex)
                return string.Join(";", parts[0], int.Parse(parts[1]) - 1, parts[2]);
            if (int.Parse(parts[1]) == link.TrainLinkIndex)
                throw new Exception("Found unexpected train qid " + qid);
            return qid;
        }

        foreach (var transition in Transitions)
        {
            transition.First = DoTraReplace(transition.First);
            transition.Next = DoTraReplace(transition.Next);
        }
            
        link.ParentTrain.RemoveLink(link);
    }

    #endregion

    /// <inheritdoc />
    public Timetable Clone() => DeepCloner.CloneObject(this);

    [DebuggerStepThrough]
    public override string ToString() => string.Join(" | ", GetRoutes().Select(r => r.GetRouteName()));
}

public class AmbiguousTransitionException : Exception
{
    public AmbiguousTransitionException(string? message) : base(message) { }
}
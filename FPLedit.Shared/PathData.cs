﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace FPLedit.Shared;

/// <summary>
/// Provides a convenient API to work with fixed paths.
/// </summary>
[Templating.TemplateSafe]
public class PathData : ISortedStations
{
    protected PathEntry[] Entries { get; init; }
    protected Station[] RawPath { get; init; }
    public PathEntry[] PathEntries => Entries;

    private readonly Timetable tt;

    /// <summary>
    /// Creates a new PathData instance with the given stations.
    /// </summary>
    public PathData(Timetable tt, IEnumerable<Station> path) : this(tt)
    {
        Entries = Init(path.ToArray(), (s, r) => new PathEntry(s, r));
        RawPath = Entries.Select(e => e.Station).ToArray();
    }

    protected PathData(Timetable tt)
    {
        this.tt = tt;
        Entries = Array.Empty<PathEntry>();
        RawPath = Array.Empty<Station>();
    }

    internal PathData(Route route, Timetable tt)
    {
        this.tt = tt;
        Entries = route.Stations.Select(s => new PathEntry(s, route.Index)).ToArray();
        RawPath = Entries.Select(e => e.Station).ToArray();
    }

    /// <summary>
    /// Initialization method. Can be used to implement custom flavoured PathData types.
    /// </summary>
    /// <param name="path">The stations of this path.</param>
    /// <param name="instanciator">A custom instanciator used to create the path entries.</param>
    /// <typeparam name="T">The type of entries used.</typeparam>
    /// <returns></returns>
    protected PathEntry[] Init<T>(IReadOnlyList<Station> path, Func<Station, int, T> instanciator) where T : PathEntry
    {
        int lastRoute = -1;
        int idx = 0;
        return path.Select(sta =>
        {
            var next = (idx < path.Count - 1) ? path[idx + 1] : null;
            int route = next != null ? tt.GetDirectlyConnectingRoute(sta, next) : lastRoute;
            lastRoute = route;
            idx++;
            return instanciator(sta, route);
        }).Cast<PathEntry>().ToArray();
    }

    /// <summary>
    /// Returns the next station after the given station, following this path, or null.
    /// </summary>
    public Station? NextStation(Station sta)
        => RawPath.SkipWhile(s => s != sta).Skip(1).FirstOrDefault();

    /// <summary>
    /// Returns the previous station before the given station, following this path, or null.
    /// </summary>
    public Station? PreviousStation(Station sta)
        => RawPath.TakeWhile(s => s != sta).LastOrDefault();

    /// <summary>
    /// Get the route this path exits the given station on.
    /// </summary>
    public int GetExitRoute(Station sta)
    {
        if (sta == RawPath.LastOrDefault())
            return Timetable.UNASSIGNED_ROUTE_ID;
        var next = NextStation(sta);
        if (next == null)
            return Timetable.UNASSIGNED_ROUTE_ID;
        return tt.GetDirectlyConnectingRoute(sta, next);
    }

    /// <summary>
    /// Get the route this path enters the given station on.
    /// </summary>
    public int GetEntryRoute(Station sta)
    {
        if (sta == RawPath.FirstOrDefault())
            return Timetable.UNASSIGNED_ROUTE_ID;
        var previous = PreviousStation(sta);
        if (previous == null)
            return Timetable.UNASSIGNED_ROUTE_ID;
        return tt.GetDirectlyConnectingRoute(sta, previous);
    }

    private int GetEntryRoute2(Station sta, Station? previous)
    {
        if (previous == null)
            return Timetable.UNASSIGNED_ROUTE_ID;
        return tt.GetDirectlyConnectingRoute(sta, previous);
    }

    /// <summary>
    /// Returns whether this path contains the given station.
    /// </summary>
    public bool ContainsStation(Station sta) => RawPath.Contains(sta);

    /// <summary>
    /// Returns whether this path connects the two given stations directly.
    /// </summary>
    public bool IsDirectlyConnected(Station sta1, Station sta2)
    {
        for (int i = 0; i < RawPath.Length; i++)
        {
            if (RawPath[i] != sta1)
                continue;
            if (i > 0 && RawPath[i - 1] == sta2)
                return true;
            if (i < RawPath.Length - 1 && RawPath[i + 1] == sta2)
                return true;
        }
        return false;
    }

    /// <summary>
    /// Returns +/-<paramref name="radius"/> stations around the given <paramref name="center"/> station, following
    /// the order of the current path.
    /// </summary>
    /// <remarks>
    /// <para>The result will be silently truncated, if there are less than <paramref name="radius"/> stations on
    /// either side of the <paramref name="center"/> station.</para>
    /// <para>If the station does not exist on the current route, an empty array will be returned.</para>
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">The radius was not positive.</exception>
    public Station[] GetSurroundingStations(Station center, int radius)
    {
        if (radius < 0)
            throw new ArgumentOutOfRangeException(nameof(radius));

        var centerIndex = Array.IndexOf(RawPath, center);
        if (centerIndex < 0)
            return Array.Empty<Station>(); // Not in the current path

        var leftIndex = Math.Max(centerIndex - radius, 0);
        var rightIndex = Math.Min(centerIndex + radius, RawPath.Length - 1) + 1;

        return RawPath[leftIndex..rightIndex];
    }

    /// <summary>
    /// Returns only the raw stations used in this path, without any additional information.
    /// </summary>
    public Station[] GetRawPath() => RawPath;

    /// <summary>
    /// Checks whether the current path is valid in the sense, that it does not contain any station multiple times.
    /// </summary>
    public bool IsValidUncollapsed() => RawPath.Distinct().Count() == RawPath.Length;

    /// <summary>
    /// Get a monotonically increasing position of all station along this path.
    /// </summary>
    public Dictionary<Station, float> GetPositionsAlongPath()
    {
        var pos = new Dictionary<Station, float>();
        var p = 0.0f;
        Station? last = null;
        foreach (var pe in RawPath)
        {
            if (last != null)
            {
                var route = GetEntryRoute2(pe, last);
                p += Math.Abs(pe.Positions.GetPosition(route)!.Value - last.Positions.GetPosition(route)!.Value);
            }

            pos.Add(pe, p);
            last = pe;
        }

        return pos;
    }

    public bool IsValidPathIntegrity()
    {
        Station? last = null;
        foreach (var pe in RawPath)
        {
            if (last != null)
            {
                var route = GetEntryRoute2(pe, last);
                if (route == -1) return false;
            }
            last = pe;
        }

        return true;
    }

    /// <summary>
    /// Get an empty <see cref="PathData"/>-Instance.
    /// </summary>
    public static PathData Empty(Timetable tt) => new PathData(tt, Array.Empty<Station>());
}

/// <summary>
/// Specific PathData, initialized with just a a train, containing the whole path &amp; time entries for this train.
/// </summary>
[Templating.TemplateSafe]
public class TrainPathData : PathData
{
    public new TrainPathEntry[] PathEntries { get; }

    public TrainPathData(Timetable tt, ITrain train) : base(tt)
    {
        var path = train.GetPath().ToArray();
        var arrDeps = train.GetArrDepsUnsorted();

        Entries = Init(path, (s, r) =>
        {
            arrDeps.TryGetValue(s, out var ardp);
            return new TrainPathEntry(s, ardp, r);
        });
        PathEntries = Entries.OfType<TrainPathEntry>().ToArray();
        RawPath = Entries.Select(e => e.Station).ToArray();
    }
}

/// <summary>
/// Entry of a default <see cref="PathData"/> structure. Represents one station on a specific route.
/// </summary>
[Templating.TemplateSafe]
public record PathEntry(Station Station, int RouteIndex);

/// <summary>
/// Entry of a <see cref="TrainPathData"/>. Represents one station on a specific route with attached time entry data.
/// </summary>
[Templating.TemplateSafe]
public record TrainPathEntry(Station Station, ArrDep? ArrDep, int RouteIndex) : PathEntry(Station, RouteIndex);
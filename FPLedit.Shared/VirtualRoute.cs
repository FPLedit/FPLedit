using System;
using System.Collections.Generic;
using System.Linq;
using FPLedit.Shared.Helpers;

namespace FPLedit.Shared;

public class VirtualRoute
{
    private readonly Timetable tt;

    private readonly Station start;
    private readonly Station end;
    private readonly Station[] waypoints;

    public int Index { get; }

    public static IEnumerable<VirtualRoute> GetVRoutes(Timetable tt)
    {
        if (tt.Type == TimetableType.Linear)
            yield break; // We have no virtual routes on linear timetables.

        var vroutesdef = tt.GetAttribute("fpl-vroutes","");
        var vroutes = vroutesdef!
            .Split(';', StringSplitOptions.RemoveEmptyEntries)
            .Select(v => v
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse).ToArray())
            .ToArray();

        var newdef = new List<string>();
        var index = Timetable.UNASSIGNED_ROUTE_ID;
        foreach (var vr in vroutes)
        {
            if (vr.Length < 2)
                continue;
            var vs = vr.Select(tt.GetStationById).ToArray();
            if (vs.Any(s => s == null))
                continue;
            yield return new VirtualRoute(tt, vs[0]!, vs[^1]!, vs[1..^1]!, --index);

            newdef.Add(string.Join(",", vr));
        }

        tt.SetAttribute("fpl-vroutes", string.Join(";", newdef));
    }

    public static VirtualRoute? GetVRoute(Timetable tt, int vRouteIdx)
    {
        if (tt.Type == TimetableType.Linear)
            throw new TimetableTypeNotSupportedException(TimetableType.Linear, "virtual routes");

        var idx = -vRouteIdx + Timetable.UNASSIGNED_ROUTE_ID - 1;
        var vroutes = GetVRoutes(tt).ToArray();
        return vroutes.Length > idx ? vroutes[idx] : null;
    }

    public static Func<PathData> GetPathDataMaybeVirtual(Timetable tt, int routeIdx)
    {
        if (routeIdx > Timetable.UNASSIGNED_ROUTE_ID)
            return () => tt.GetRoute(routeIdx).ToPathData(tt);
        return GetVRoute(tt, routeIdx)!.GetPathData;
    }

    public static void DeleteVRoute(VirtualRoute route)
    {
        var tt = route.tt;

        var olddef = string.Join(",", new[] { route.start }.Concat(route.waypoints).Concat(new[] { route.end }).Select(s => s.Id));

        var vroutesdef = tt.GetAttribute("fpl-vroutes","");
        var vroutes = vroutesdef.Split(';', StringSplitOptions.RemoveEmptyEntries).ToList();
        vroutes.RemoveAll(s => s == olddef);
        tt.SetAttribute("fpl-vroutes", string.Join(";", vroutes));
    }

    public static void CreateVRoute(Timetable tt, Station start, Station end, Station[] waypoints)
    {
        if (tt.Type == TimetableType.Linear)
            throw new TimetableTypeNotSupportedException(TimetableType.Linear, "virtual routes");

        var newdef = string.Join(",", new[] { start }.Concat(waypoints).Concat(new[] { end }).Select(s => s.Id));

        var vroutesdef = tt.GetAttribute("fpl-vroutes","");
        var vroutes = vroutesdef!.Split(';', StringSplitOptions.RemoveEmptyEntries).ToList();
        vroutes.Add(newdef);
        tt.SetAttribute("fpl-vroutes", string.Join(";", vroutes));
    }

    private VirtualRoute(Timetable tt, Station start, Station end, Station[] waypoints, int index)
    {
        if (tt.Type == TimetableType.Linear)
            throw new TimetableTypeNotSupportedException(TimetableType.Linear, "virtual routes");

        this.tt = tt;
        this.start = start;
        this.end = end;
        this.waypoints = waypoints;
        this.Index = index;
    }

    public PathData GetPathData()
    {
        var pf = new Pathfinder(tt);
        var stas = pf.GetPath(start, end, waypoints);
        var pd = new PathData(tt, stas);
        if (pd.IsValidUncollapsed())
            throw new VirtualRouteInvalidEception(T._("Ungültige (zusammengefallene) virtuelle Strecke!"));
        return pd;
    }

    public string GetRouteName()
        => "[v] " + string.Join(" - ", new[] { start }.Concat(waypoints).Concat(new[] { end }).Select(s => s.SName));
}

public class VirtualRouteInvalidEception : Exception
{
    public VirtualRouteInvalidEception(string? message) : base(message) { }
}
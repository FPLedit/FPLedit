using System;
using FPLedit.Shared;
using System.Linq;
using System.Collections.Generic;
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ClassNeverInstantiated.Global

namespace FPLedit.Kursbuch.Templates;

public sealed class TemplateHelper
{
    private record TrainCache(Station[] Path, Dictionary<Station, ArrDep> ArrDeps);

    private readonly Timetable tt;
    private readonly Dictionary<ITrain, TrainCache> trainPathCache = new();

    private readonly FilterRule[] trules, srules;

    public TemplateHelper(Timetable tt)
    {
        this.tt = tt;

        var filterable = Plugin.FilterRuleContainer;

        trules = filterable.LoadTrainRules(tt).ToArray();
        srules = filterable.LoadStationRules(tt).ToArray();
    }

    public Station[] GetStations(Route route, TrainDirection dir)
    {
        var stas = route.Stations.Where(s => srules.All(r => !r.Matches(s)))
            .ToArray();
        if (dir == TrainDirection.ta)
            return stas.Reverse().ToArray();
        return stas;
    }

    public ITrain[] GetTrains(Route route, TrainDirection direction)
    {
        var routeStations = GetStations(route, direction);
        var firstTimes = new Dictionary<ITrain, TimeEntry?[]>();

        foreach (var t in tt.Trains)
        {
            if (trules.Any(r => r.Matches(t)))
                continue;

            if (tt.Type == TimetableType.Linear) // Züge in linearen Fahrplänen sind recht einfach
            {
                if (t.Direction != direction)
                    continue;

                var times = new TimeEntry?[routeStations.Length];
                for (var i = 0; i < routeStations.Length; i++)
                {
                    var a = t.GetArrDep(routeStations[i]);
                    times[i] = a.HasMinOneTimeSet ? a.FirstSetTime : null;
                }

                if (times.All(te => te == null)) continue;
                firstTimes.Add(t, times);
            }
            else
            {
                if (!trainPathCache.TryGetValue(t, out var path))
                {
                    var a = t.GetArrDepsUnsorted();
                    var p = a
                        .Where(s => s.Value.HasMinOneTimeSet)
                        .OrderBy(s => s.Value.FirstSetTime)
                        .Select(s => s.Key)
                        .ToArray();
                    path = new TrainCache(p, a);
                    trainPathCache[t] = path;
                }
                var sortedStopsOnRoute = routeStations.Intersect(path.Path).ToArray();

                if (sortedStopsOnRoute.Length == 0) // The train does not stop on this route, ignore.
                    continue;

                if (Array.IndexOf(routeStations, sortedStopsOnRoute[0]) >= Array.IndexOf(routeStations, sortedStopsOnRoute[^1])) // exclude other direction of this train.
                    continue;

                var times = new TimeEntry?[routeStations.Length];
                for (var i = 0; i < routeStations.Length; i++)
                {
                    var a = sortedStopsOnRoute.Contains(routeStations[i]) ? path.ArrDeps[routeStations[i]] : null;
                    times[i] = (a?.HasMinOneTimeSet ?? false) ? a.FirstSetTime : null;
                }

                if (times.All(te => te == null)) continue;
                firstTimes.Add(t, times);
                // else: Not needed as routeStations are already sorted according to direction.
            }
        }

        // Incremental sort, from last station backwards.
        var trains = firstTimes.Keys.ToList();
        for (var i = routeStations.Length - 1; i >= 0; i--)
        {
            trains.Sort(((train1, train2) =>
            {
                var te1 = firstTimes[train1][i];
                var te2 = firstTimes[train2][i];
                if (!te1.HasValue || !te2.HasValue) return 0;
                return te1.Value.CompareTo(te2.Value);
            }));
        }

        return trains.ToArray();
    }

    public string GetRouteName(Route r, TrainDirection dir)
    {
        var stas = GetStations(r, dir);
        return Shared.Templating.TemplateOutput.SafeHtml(stas.FirstOrDefault()?.SName + " - " + stas.LastOrDefault()?.SName);
    }
}
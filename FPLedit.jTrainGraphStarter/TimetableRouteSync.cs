﻿using FPLedit.Shared;
using FPLedit.Shared.Filetypes;
using System.Collections.Generic;
using System.Linq;

namespace FPLedit.jTrainGraphStarter;

internal sealed class TimetableRouteSync : BaseConverterFileType
{
    private readonly Timetable orig;
    private readonly TimetableVersion origVersion;
    private readonly int routeIndex;

    private readonly List<ITrain> trainMap;

    public TimetableRouteSync(Timetable tt, int routeIndex)
    {
        orig = tt;
        origVersion = tt.Version;
        this.routeIndex = routeIndex;
        trainMap = new List<ITrain>();
    }

    #region Network -> Route

    public Timetable GetRouteTimetable(TimetableVersion targetVersion)
    {
        var copy = orig.Clone();

        var route = copy.GetRoute(routeIndex);

        for (int si = 0; si < copy.Stations.Count; si++)
        {
            var sta = copy.Stations[si];

            if (!route.Stations.Contains(sta))
            {
                copy.RemoveStation(sta);
                si--;
            }
        }

        // XML-Elemente wirklich sortieren. In copy.Stations wird nicht zurückgesynct,
        // daher eine eigene sortierte Liste für später
        var sortedStations = copy.Stations.OrderBy(s => s.Positions.GetPosition(routeIndex)).ToList();

        var stasElm = copy.Children.First(x => x.XName == "stations");
        stasElm.Children.Clear();
        stasElm.Children.InsertRange(0, sortedStations.Select(s => s.XMLEntity).OrderBy(c =>
        {
            if (c.XName == "sta")
                return copy.Stations.FirstOrDefault(s => s.XMLEntity == c)?.Positions.GetPosition(routeIndex);
            return null;
        }));

        int syncId = 0;
        for (int ti = 0; ti < copy.Trains.Count; ti++)
        {
            var tra = copy.Trains[ti];
            var ardps = tra.GetArrDepsUnsorted();
            var path = tra.GetPath();

            var pf = path.FirstOrDefault();
            var pl = path.LastOrDefault();
            var isEmpty = pf != null && pl != null && ((ardps[pf].Arrival != default && ardps[pf].Departure == default) || (ardps[pl].Departure != default && ardps[pl].Arrival == default));
            if (ardps.Count == 0 || ardps.All(a => !a.Value.HasMinOneTimeSet) || isEmpty) // Dieser Zug berührt diese Route nicht
            {
                copy.RemoveTrain(tra);
                ti--;
                continue;
            }
            tra.SetAttribute("fpl-sync-id", syncId++.ToString());
            trainMap.Add(tra); // Der Index wird immer um 1 hochegzählt, daher brauchts hier kein Dictionary

            if (tra is IWritableTrain wt)
            {
                // Fahrtzeiteneinträge setzen
                wt.Children.Clear();
                wt.AddAllArrDeps(sortedStations);
                foreach (var ardp in ardps)
                    if (sortedStations.Contains(ardp.Key))
                        wt.GetArrDep(ardp.Key).ApplyCopy(ardp.Value);
            }

            // Lineare Fahrtrichtung bestimmen
            var sta1 = ardps.FirstOrDefault().Key;
            var sta2 = ardps.LastOrDefault().Key;

            var dir = TrainDirection.ti;
            if (sta1 != sta2)
            {
                if (sortedStations.IndexOf(sta1) > sortedStations.IndexOf(sta2))
                    dir = TrainDirection.ta;
            }
            else if (sortedStations.IndexOf(sta1) == sortedStations.Count - 1)
                dir = TrainDirection.ta;

            tra.XMLEntity.XName = dir.ToString();
        }

        // Am Ende die Kilometer & anderen Attribute auf den linearen Stil setzen
        foreach (var sta in copy.Stations)
            ConvertStationNetToLin(sta, routeIndex);

        copy.SetVersion(targetVersion); // Wir gehen aus dem Extended-Modus raus

        return copy;
    }

    #endregion

    #region Route -> Network

    public void SyncBack(Timetable singleRoute)
    {
        orig.Attributes = AttrDiff(orig, singleRoute);
        orig.SetVersion(origVersion); // Wieder in Netzwerk-Modus wechseln

        foreach (var sta in orig.Stations)
        {
            var srSta = singleRoute.Stations.FirstOrDefault(s => s.GetAttribute<int>("fpl-id") == sta.Id);
            if (srSta == null)
                continue;

            //TODO: Better method to remove all known attributes?
            foreach (var a in new[] { "km", "kml", "kmr", "fpl-rt", "fpl-id", "fpl-wl", "fpl-vmax", "fpl-tp", "fpl-cd", "tr", "dTi", "dTa" })
                srSta.RemoveAttribute(a); // Alte Angaben entfernen

            sta.Attributes = AttrDiff(sta, srSta);
        }

        foreach (var tra in orig.Trains)
        {
            var syncId = trainMap.IndexOf(tra);
            if (syncId == -1)
                continue;

            var srTra = singleRoute.Trains.FirstOrDefault(t => t.GetAttribute<int>("fpl-sync-tra-id") == syncId);
            if (srTra == null)
                continue; // Unexpected, but we do not want to crash with jTG interop.

            srTra.RemoveAttribute("fpl-sync-id");
            tra.Attributes = AttrDiff(tra, srTra);
        }
    }

    private Dictionary<string, string> AttrDiff(IEntity xold, IEntity xnew)
    {
        var result = new[] { xold.Attributes, xnew.Attributes }.SelectMany(dict => dict)
            .ToLookup(pair => pair.Key, pair => pair.Value)
            .ToDictionary(group => group.Key, group => group.Last());
        return result;
    }

    #endregion
}
using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace FPLedit.jTrainGraphStarter
{
    internal class TimetableRouteSync
    {
        private Timetable orig;
        private int routeIndex;

        private List<Train> trainMap;

        public TimetableRouteSync(Timetable tt, int routeIndex)
        {
            orig = tt;
            this.routeIndex = routeIndex;
            trainMap = new List<Train>();
        }

        #region Network -> Route

        public Timetable GetRouteTimetable(TimetableVersion targetVersion)
        {
            var copy = orig.Clone();
            copy.SetAttribute("version", ((int)targetVersion).ToString("000")); // Wir gehen aus dem Extended-Modus raus

            var route = copy.GetRoute(routeIndex);

            for (int si = 0; si < copy.Stations.Count; si++)
            {
                var sta = copy.Stations[si];
                if (!route.Stations.Contains(sta))
                {
                    copy.RemoveStation(sta);
                    si--;
                    continue;
                }
            }

            // XML-Elemente wirklich sortieren. In copy.Stations wird nicht zurückgesynct,
            // daher eine eigene sortierte Liste für später
            var sortedStations = copy.Stations.OrderBy(s => s.Positions.GetPosition(routeIndex)).ToList();

            var stasElm = copy.Children.First(x => x.XName == "stations");
            stasElm.Children = stasElm.Children.OrderBy(c =>
            {
                if (c.XName == "sta")
                    return copy.Stations.FirstOrDefault(s => s.XMLEntity == c)?.Positions.GetPosition(routeIndex);
                return null;
            }).ToList();


            int syncId = 0;
            for (int ti = 0; ti < copy.Trains.Count; ti++)
            {
                var tra = copy.Trains[ti];
                var ardps = tra.GetArrDeps();

                if (ardps.Count == 0) // Dieser Zug berührt diese Route nicht
                {
                    copy.RemoveTrain(tra);
                    ti--;
                    continue;
                }
                tra.SetAttribute("fpl-sync-id", syncId++.ToString());
                trainMap.Add(tra); // Der Index wird immer um 1 hochegzählt, daher brauchts hier kein Dictionary

                // Fahrtzeiteneinträge setzen
                tra.Children.Clear();
                tra.AddAllArrDeps(sortedStations);
                foreach (var ardp in ardps)
                    tra.SetArrDep(ardp.Key, ardp.Value);

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

                if (dir == TrainDirection.ta)
                    tra.Children.Reverse();

                tra.XMLEntity.XName = dir.ToString();
            }

            // Am Ende die Kilometer auf den linearen Stil setzen
            foreach (var sta in copy.Stations)
            {
                float km = sta.Positions.GetPosition(routeIndex).Value;
                var pos = km.ToString("0.0", CultureInfo.InvariantCulture);

                if (targetVersion == TimetableVersion.JTG2_x)
                    sta.SetAttribute("km", pos);
                else if (targetVersion == TimetableVersion.JTG3_0)
                {
                    sta.SetAttribute("kml", pos);
                    sta.SetAttribute("kmr", pos);
                    sta.RemoveAttribute("km"); // Alte Netzwerk-Form entfernen
                }
            }

            return copy;
        }

        #endregion

        #region Route -> Network

        public void SyncBack(Timetable singleRoute)
        {
            singleRoute.SetAttribute("version", "100"); // Wieder in Netzwerk-Modus wechseln
            orig.Attributes = AttrDiff(orig, singleRoute);

            foreach (var sta in orig.Stations)
            {
                var srSta = singleRoute.Stations.FirstOrDefault(s => s.GetAttribute<int>("fpl-id") == sta.Id);
                if (srSta == null)
                    continue;
                srSta.RemoveAttribute("km"); // Alte km-Angabe entfernen

                sta.Attributes = AttrDiff(sta, srSta);
            }

            foreach (var tra in orig.Trains)
            {
                var syncId = trainMap.IndexOf(tra);
                if (syncId == -1)
                    continue;

                var srTra = singleRoute.Trains.FirstOrDefault(t => t.GetAttribute<int>("fpl-sync-tra-id") == syncId);

                srTra.RemoveAttribute("fpl-sync-id");
                tra.Attributes = AttrDiff(tra, srTra);
            }
        }

        private Dictionary<string, string> AttrDiff(Entity xold, Entity xnew)
        {
            var result = new[] { xold.Attributes, xnew.Attributes }.SelectMany(dict => dict)
                         .ToLookup(pair => pair.Key, pair => pair.Value)
                         .ToDictionary(group => group.Key, group => group.Last());
            return result;
        }

        #endregion
    }
}

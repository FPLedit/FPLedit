using FPLedit.Shared;
using System;
using System.Linq;
using System.Collections.Generic;
using FPLedit.Shared.Helpers;

namespace FPLedit.Aushangfahrplan.Templates
{
    public sealed class TemplateHelper
    {
        private readonly Timetable tt;
        private readonly NetworkHelper nh;
        private readonly FilterRule[] trules, srules;

        public TemplateHelper(Timetable tt)
        {
            this.tt = tt;
            nh = new NetworkHelper(tt);

            var filterable = Plugin.FilterRuleContainer;

            trules = filterable.LoadTrainRules(tt).ToArray();
            srules = filterable.LoadStationRules(tt).ToArray();
        }

        public Station[] GetStations()
        {
            return tt.Stations
                .Where(s => srules.All(r => !r.Matches(s)))
                .Where(s => GetTrains(s).Any())
                .ToArray();
        }

        private IEnumerable<ITrain> GetTrains(Station sta)
        {
            return tt.Trains.Where(t => t.GetPath().Contains(sta)
                                        && t.GetArrDep(sta).Departure != default
                                        && trules.All(r => !r.Matches(t)))
                .OrderBy(t => t.GetArrDep(sta).Departure);
        }

        #region Last stations

        public Station[] GetLastStations(TrainDirection dir, Station sta, IEnumerable<object> trainsInThisDirObj)
        {
            var trainsInThisDir = trainsInThisDirObj.Cast<Train>(); // From JS.
            if (tt.Type == TimetableType.Linear)
            {
                var lSta = tt.GetLinearStationsOrderedByDirection(dir).LastOrDefault();
                if (lSta != sta)
                    return new[] { lSta };
                return Array.Empty<Station>();
            }

            // Alle Stationen in Zügen dieser Richtung, die nach dieser Station folgen
            var stasInTrains = trainsInThisDir.SelectMany(t => t.GetArrDepsUnsorted().Where(a => a.Value.HasMinOneTimeSet).Select(kvp => kvp.Key).SkipWhile(s => s != sta)).ToArray();

            var connectionNodes = sta.ParentTimetable.Stations.Where(s => s.Routes.Length > 1);
            var visitedConnectionNodes = connectionNodes.Intersect(stasInTrains); // Eine Hälfte der "Richtungsangaben": Verbindungsknoten im Netz

            var visitedRoutes = stasInTrains.SelectMany(s => s.Routes).Distinct();

            var stasAfter = new List<Station>();
            stasAfter.AddRange(visitedConnectionNodes);

            foreach (var rt in visitedRoutes)
            {
                var route = sta.ParentTimetable.GetRoute(rt).Stations;
                stasAfter.Add(route.First());
                stasAfter.Add(route.Last());
            }

            return stasAfter.Distinct().OrderBy(s => s.SName).ToArray();
        }
        #endregion

        public ITrain[] GetTrains(TrainDirection dir, Station sta) => nh.GetTrains(GetTrains(sta), dir, sta).ToArray();
    }
}
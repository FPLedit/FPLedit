using FPLedit.Aushangfahrplan.Model;
using FPLedit.Shared;
using System;
using System.Linq;
using System.Collections.Generic;
using FPLedit.Shared.DefaultImplementations;

namespace FPLedit.Aushangfahrplan.Templates
{
    public class TemplateHelper
    {
        private readonly Timetable tt;
        private readonly FilterRule[] trules, srules;

        public TemplateHelper(Timetable tt)
        {
            this.tt = tt;

            var filterable = new BasicFilterableProvider("Aushangfahrplan", AfplAttrs.GetAttrs, AfplAttrs.CreateAttrs);

            trules = filterable.LoadTrainRules(tt).ToArray();
            srules = filterable.LoadStationRules(tt).ToArray();
        }

        public Station[] GetStations()
        {
            var x = tt.Stations
                .Where(s => srules.All(r => !r.Matches(s)))
                .Where(s => GetTrains(s).Length > 0)
                .ToArray();
            return x;
        }

        private Train[] GetTrains(Station sta)
        {
            return tt.Trains.Where(t => t.GetPath().Contains(sta)
                && t.GetArrDep(sta).Departure != default
                && trules.All(r => !r.Matches(t)))
                .OrderBy(t => t.GetArrDep(sta).Departure)
                .ToArray();
        }

        #region Last stations

        public Station[] GetLastStations(TrainDirection dir, Station sta, IEnumerable<object> trainsInThisDirObj)
        {
            var trainsInThisDir = trainsInThisDirObj.Cast<Train>(); // From JS.
            if (tt.Type == TimetableType.Linear)
            {
                var lSta = tt.GetStationsOrderedByDirection(dir).LastOrDefault();
                if (lSta != sta)
                    return new[] { lSta };
                return Array.Empty<Station>();
            }

            // Alle Stationen in Zügen dieser Richtung, die nach dieser Station folgen
            var stasInTrains = trainsInThisDir.SelectMany(t => t.GetArrDeps().Where(a => a.Value.HasMinOneTimeSet).Select(kvp => kvp.Key).SkipWhile(s => s != sta)).ToArray();

            var connectionNodes = sta._parent.Stations.Where(s => s.Routes.Length > 1);
            var visitedConnectionNodes = connectionNodes.Intersect(stasInTrains); // Eine Hälfte der "Richtungsangaben": Verbindungsknoten im Netz

            var visitedRoutes = stasInTrains.SelectMany(s => s.Routes).Distinct();

            var stasAfter = new List<Station>();
            stasAfter.AddRange(visitedConnectionNodes);

            foreach (var rt in visitedRoutes)
            {
                var route = sta._parent.GetRoute(rt).GetOrderedStations();
                stasAfter.Add(route.First());
                stasAfter.Add(route.Last());
            }

            return stasAfter.Distinct().OrderBy(s => s.SName).ToArray();
        }
        #endregion

        #region Trains at station

        public Train[] GetTrains(TrainDirection dir, Station sta)
        {
            var stasAfter = GetStationsInDir(dir, sta);
            var stasBefore = GetStationsInDir(dir == TrainDirection.ta ? TrainDirection.ti : TrainDirection.ta, sta);

            return GetTrains(sta).Where(t =>
            {
                var p = t.GetPath();
                var ardeps = t.GetArrDeps();

                var nsta = p.Where(s => stasAfter.Contains(s)).FirstOrDefault(s => ardeps[s].HasMinOneTimeSet);
                if (nsta == null)
                {
                    var lsta = p.Where(s => stasBefore.Contains(s)).FirstOrDefault(s => ardeps[s].HasMinOneTimeSet);
                    if (lsta == null)
                        return false;
                    var ltime = ardeps[lsta].FirstSetTime;
                    return ltime < ardeps[sta].Departure;
                }
                var ntime = ardeps[nsta].FirstSetTime;
                return ntime > ardeps[sta].Departure;
            }).ToArray();
        }

        private Station[] GetStationsInDir(TrainDirection dir, Station sta)
        {
            if (tt.Type == TimetableType.Linear)
            {
                var stas = tt.GetStationsOrderedByDirection(dir);
                return stas.Skip(stas.IndexOf(sta) + 1).ToArray();
            }
            var stasAfter = new List<Station>();
            foreach (var rt in sta.Routes)
            {
                var route = tt.GetRoute(rt);
                var pos = sta.Positions.GetPosition(rt);
                var nextStations = dir == TrainDirection.ti ?
                    route.Stations.Where(s => s.Positions.GetPosition(rt) > pos) : // ti
                    route.Stations.Where(s => s.Positions.GetPosition(rt) < pos); // ta
                stasAfter.AddRange(nextStations);
            }
            return stasAfter.ToArray();
        }

        #endregion
    }
}
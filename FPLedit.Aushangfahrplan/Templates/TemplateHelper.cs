using FPLedit.Aushangfahrplan.Model;
using FPLedit.Shared;
using System;
using System.Linq;
using System.Collections.Generic;

namespace FPLedit.Aushangfahrplan.Templates
{
    public class TemplateHelper
    {
        public Timetable TT { get; private set; }

        private FilterRule[] trules, srules;

        public TemplateHelper(Timetable tt)
        {
            TT = tt;

            var filterable = new Forms.FilterableHandler();

            trules = filterable.LoadTrainRules(tt).ToArray();
            srules = filterable.LoadStationRules(tt).ToArray();
        }

        public Station[] GetStations()
        {
            var x = TT.Stations
                .Where(s => srules.All(r => !r.Matches(s)))
                .Where(s => GetTrains(s).Length > 0)
                .ToArray();
            return x;
        }

        private Train[] GetTrains(Station sta)
        {
            return TT.Trains.Where(t => t.GetPath().Contains(sta)
                && t.GetArrDep(sta).Departure != default
                && trules.All(r => !r.Matches(t)))
                .OrderBy(t => t.GetArrDep(sta).Departure)
                .ToArray();
        }

        #region Last stations

        public Station[] GetLastStations(TrainDirection dir, Station sta)
        {
            if (TT.Type == TimetableType.Linear)
            {
                var lSta = TT.GetStationsOrderedByDirection(dir).LastOrDefault();
                if (lSta != sta)
                    return new[] { lSta };
                return new Station[0];
            }

            var stasAfter = new List<Station>();

            var routes = sta.Routes;
            foreach (var rt in routes)
            {
                var route = TT.GetRoute(rt);
                var stas = route.GetOrderedStations();
                var pos = sta.Positions.GetPosition(rt);
                var nextStations = dir == TrainDirection.ti ?
                    stas.Where(s => s.Positions.GetPosition(rt) > pos) : // ti
                    stas.Where(s => s.Positions.GetPosition(rt) < pos).Reverse(); // ta

                if (!nextStations.Any())
                    continue;

                stasAfter.Add(nextStations.Last());

                foreach (var s in nextStations)
                {
                    if (s.Routes.Count() > 1)
                        stasAfter.AddRange(GetSubLastStations(s, rt));
                }
            }
            return stasAfter.ToArray();
        }

        private List<Station> GetSubLastStations(Station sta, int originatingRoute)
        {
            var stasAfter = new List<Station>();
            foreach (var rt in sta.Routes)
            {
                if (rt == originatingRoute)
                    continue;

                var route = TT.GetRoute(rt);
                var stations = route.GetOrderedStations();
                if (!stations.Any())
                    continue;

                if (stations.Last() == sta)
                    stations.Reverse();

                stasAfter.Add(stations.Last());

                foreach (var s in stations)
                {
                    if (s.Routes.Count() > 1 && s != sta)
                        stasAfter.AddRange(GetSubLastStations(s, rt));
                }
            }
            return stasAfter;
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
            if (TT.Type == TimetableType.Linear)
            {
                var stas = TT.GetStationsOrderedByDirection(dir);
                return stas.Skip(stas.IndexOf(sta) + 1).ToArray();
            }
            var stasAfter = new List<Station>();
            foreach (var rt in sta.Routes)
            {
                var route = TT.GetRoute(rt);
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
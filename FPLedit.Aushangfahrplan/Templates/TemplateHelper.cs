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

        public Train[] GetTrains(Station sta)
        {
            return TT.Trains.Where(t => t.GetPath().Contains(sta)
                && t.GetArrDep(sta).Departure != default(TimeSpan)
                && trules.All(r => !r.Matches(t)))
                .ToArray();
        }

        public Station[] GetStationsInDir(TrainDirection dir, Station sta, bool onlyFirst = true)
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
                var pos = sta.Positions.GetPosition(rt);
                var nextStations = dir == TrainDirection.ti ?
                    route.Stations.Where(s => s.Positions.GetPosition(rt) > pos) : // ti
                    route.Stations.Where(s => s.Positions.GetPosition(rt) < pos); // ta
                if (onlyFirst && nextStations.Any())
                    stasAfter.Add(nextStations.Last());
                else
                    stasAfter.AddRange(nextStations);
            }
            return stasAfter.ToArray();
        }

        public Station[] GetStationsFromDir(TrainDirection dir, Station sta, bool onlyFirst = true)
            => GetStationsInDir(dir == TrainDirection.ta ? TrainDirection.ti : TrainDirection.ta, sta, onlyFirst);

        public Train[] GetTrains(TrainDirection dir, Station sta)
        {
            var stasAfter = GetStationsInDir(dir, sta, false);
            var stasBefore = GetStationsFromDir(dir, sta, false);
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
    }
}
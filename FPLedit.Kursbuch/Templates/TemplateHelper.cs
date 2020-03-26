using FPLedit.Shared;
using System;
using System.Linq;
using System.Collections.Generic;
using FPLedit.Kursbuch.Model;
using FPLedit.Shared.DefaultImplementations;
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable ClassNeverInstantiated.Global

namespace FPLedit.Kursbuch.Templates
{
    public sealed class TemplateHelper
    {
        private readonly Timetable tt;

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

        public Train[] GetTrains(Route route, TrainDirection direction)
        {
            var routeStations = (IList<Station>)GetStations(route, direction).ToArray();
            var firstTimes = new Dictionary<Train, TimeEntry>();

            foreach (var t in tt.Trains)
            {
                if (trules.Any(r => r.Matches(t)))
                    continue;
                
                if (tt.Type == TimetableType.Linear) // Züge in linearen Fahrplänen sind recht einfach
                {
                    if (t.Direction != direction)
                        continue;

                    var ardps = t.GetArrDepsUnsorted();
                    var firstStaion = t.GetPath().FirstOrDefault(a => ardps[a].HasMinOneTimeSet);
                    if (firstStaion == null)
                        continue; // Something weird happened...
                    firstTimes.Add(t, ardps[firstStaion].FirstSetTime);
                }
                else
                {
                    var path = t.GetPath();
                    var sortedStopsOnRoute = routeStations
                        .Intersect(path)
                        .OrderBy(s => t.GetArrDep(s).FirstSetTime)
                        .Where(s => t.GetArrDep(s).HasMinOneTimeSet)
                        .ToArray();

                    if (!sortedStopsOnRoute.Any()) // The train does not stop on this route, ignore.
                        continue;

                    if (routeStations.IndexOf(sortedStopsOnRoute.First()) < routeStations.IndexOf(sortedStopsOnRoute.Last()))
                    {
                        var time = t.GetArrDep(sortedStopsOnRoute.First()).FirstSetTime;
                        firstTimes.Add(t, time);
                    } // else: Not needed as routeStations are already sorted according to direction.
                }
            }

            return firstTimes.Keys.OrderBy(t => firstTimes[t]).ToArray();
        }

        public string GetRouteName(Route r, TrainDirection dir)
        {
            var stas = GetStations(r, dir);
            return stas.FirstOrDefault()?.SName + " - " + stas.LastOrDefault()?.SName;
        }
    }
}
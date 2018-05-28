using FPLedit.Shared;
using System;
using System.Linq;
using System.Collections.Generic;

namespace FPLedit.Kursbuch.Templates
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

        public Station[] GetStations(Route route, TrainDirection dir)
        {
            var stas = route.GetOrderedStations().Where(s => srules.All(r => !r.Matches(s)))
                .ToArray();
            if (dir == TrainDirection.ta)
                return stas.Reverse().ToArray();
            return stas;
        }

        public Train[] GetTrains(Route route, TrainDirection dir)
        {
            var stas = GetStations(route, dir).ToList();
            var tras = new List<Train>();

            foreach (var t in TT.Trains)
            {
                if (!trules.All(r => !r.Matches(t)))
                    continue;
                if (TT.Type == TimetableType.Linear) // Züge in linearen Fahrplänen sind recht einfach
                {
                    if (t.Direction == dir)
                        tras.Add(t);
                    continue;
                }

                var path = t.GetPath();
                var inters = stas.Intersect(path);
                var stas2 = inters.OrderBy(s => t.GetArrDep(s).FirstSetTime)
                    .Where(s => t.GetArrDep(s).HasMinOneTimeSet);

                if (!stas2.Any())
                    continue;

                if (stas.IndexOf(stas2.First()) < stas.IndexOf(stas2.Last()))
                    tras.Add(t);
            }

            return tras.ToArray();
        }

        public string GetRouteName(Route r, TrainDirection dir)
        {
            var stas = GetStations(r, dir);
            return stas.FirstOrDefault()?.SName + " - " + stas.LastOrDefault()?.SName;
        }
    }
}
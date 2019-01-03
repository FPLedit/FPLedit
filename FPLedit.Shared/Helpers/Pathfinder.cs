using System;
using System.Collections.Generic;
using System.Linq;

namespace FPLedit.Shared.Helpers
{
    /// <summary>
    /// Findet den kürzesten Weg zwischen zwei Stationen
    /// </summary>
    public class Pathfinder
    {
        private Timetable tt;

        public Pathfinder(Timetable tt)
        {
            this.tt = tt;
        }

        public List<Station> GetFromAToB(Station start, Station dest, params Station[] waypoints)
        {
            if (!tt.Stations.Contains(start) || !tt.Stations.Contains(dest))
                throw new ArgumentException("Start- oder Zielstation noch nicht im Fahrplan enthalten");

            var points = new List<Station>(waypoints);
            points.Insert(0, start);
            points.Add(dest);

            var ret = new List<Station>();
            for (int i = 0; i < points.Count - 1; i++)
                ret.AddRange(GetFromAToBIter(points[i], points[i + 1], new List<Station>(), new HashSet<Station>()));
            return ret;
        }

        private IEnumerable<Station> GetNextStations(Station sta)
        {
            foreach (var ri in sta.Routes)
            {
                var route = tt.GetRoute(ri).GetOrderedStations();
                var idx = route.IndexOf(sta);

                if (idx > 0)
                    yield return route[idx - 1];
                if (idx < route.Count - 1)
                    yield return route[idx + 1];
            }
        }

        private List<Station> GetFromAToBIter(Station a, Station dest, List<Station> way, HashSet<Station> visited)
        {
            if (!visited.Add(a))
                return null;

            way.Add(a);
            var orig = way;

            foreach (var sta in GetNextStations(a))
            {
                if (sta == dest)
                    way.Add(dest);
                else
                    way = GetFromAToBIter(sta, dest, way.ToList(), visited);

                if (way?.Last() == dest)
                    return way;

                way = orig;
            }
            return null;
        }
    }
}

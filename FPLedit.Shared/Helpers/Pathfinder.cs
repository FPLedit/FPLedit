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

        public List<Station> GetFromAToB(Station start, Station dest)
        {
            if (!tt.Stations.Contains(start) || !tt.Stations.Contains(dest))
                throw new ArgumentException("Start- oder Zielstation noch nicht im Fahrplan enthalten");
            visited = new List<Station>();
            var ret = new List<Station>();
            return GetFromAToBIter(start, dest, ret);
        }

        private List<Station> visited;

        private List<Station> GetFromAToBIter(Station a, Station dest, List<Station> way)
        {
            if (visited.Contains(a))
                return null;
            visited.Add(a);
            way = new List<Station>(way);
            way.Add(a);
            foreach (var ri in a.Routes)
            {
                var route = tt.GetRoute(ri);
                var stas = route.GetOrderedStations();
                var idx = stas.IndexOf(a);
                List<Station> way1 = null, way2 = null;
                if (idx > 0)
                {
                    if (stas[idx - 1] == dest)
                    {
                        way.Add(dest);
                        return way; // Gefunden
                    }
                    else
                        way1 = GetFromAToBIter(stas[idx - 1], dest, way);
                }
                if (idx < stas.Count - 1)
                {
                    if (stas[idx + 1] == dest)
                    {
                        way.Add(dest);
                        return way; // Gefunden
                    }
                    else
                        way2 = GetFromAToBIter(stas[idx + 1], dest, way);
                }
                if (way1?.Last() == dest)
                    return way1;
                if (way2?.Last() == dest)
                    return way2;

            }
            return null;
        }
    }
}

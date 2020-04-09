using System;
using System.Collections.Generic;
using System.Linq;

namespace FPLedit.Shared.Helpers
{
    /// <summary>
    /// Helper to find a path between two stations.
    /// </summary>
    [Templating.TemplateSafe]
    public sealed class Pathfinder
    {
        private readonly Timetable tt;

        public Pathfinder(Timetable tt)
        {
            this.tt = tt;
        }

        /// <summary>
        /// Find a (but not necessarily the shortest) path between two stations.
        /// </summary>
        /// <param name="start">Start station.</param>
        /// <param name="dest">Destination station.</param>
        /// <param name="waypoints">Waypoints that will be respected in the order they are passed.</param>
        /// <exception cref="ArgumentException">One or more of the stations are not part of the timetable.</exception>
        public List<Station> GetPath(Station start, Station dest, params Station[] waypoints)
        {
            var points = new List<Station>(waypoints);
            points.Insert(0, start);
            points.Add(dest);
            
            if (points.Any(p => !tt.Stations.Contains(p)))
                throw new ArgumentException("Start-, Ziel- oder Wegbpunktstation noch nicht im Fahrplan enthalten");

            var ret = new List<Station>();
            for (int i = 0; i < points.Count - 1; i++)
            {
                var part = GetFromAToB(points[i], points[i + 1]);
                ret.AddRange(part.Skip(i == 0 ? 0 : 1));
            }
            return ret;
        }

        private IEnumerable<Station> GetNextStations(Station sta)
        {
            foreach (var ri in sta.Routes)
            {
                var route = tt.GetRoute(ri).Stations;
                var idx = route.IndexOf(sta);

                if (idx > 0)
                    yield return route[idx - 1];
                if (idx < route.Count - 1)
                    yield return route[idx + 1];
            }
        }

        private List<Station> GetFromAToB(Station start, Station dest)
        {
            var visited = new HashSet<Station>();
            var queue = new Queue<Station>();
            var parents = new Dictionary<Station, Station>();
            var ways = new Dictionary<Station, Station[]>();

            queue.Enqueue(start);
            parents[start] = null;
            ways[start] = Array.Empty<Station>();

            while (queue.Any())
            {
                var rail = queue.Dequeue();

                if (rail == dest)
                    return ways[dest].SkipWhile(r => r == null).Concat(new[] { dest }).ToList();

                var last = parents[rail];
                foreach (var n in GetNextStations(rail))
                {
                    if (visited.Contains(n) || queue.Contains(n))
                        continue;

                    parents[n] = rail;
                    ways[n] = ways[last ?? start].Concat(new[] { last, rail }).ToArray();
                    queue.Enqueue(n);
                }

                visited.Add(rail);
            }

            return null;
        }
    }
}

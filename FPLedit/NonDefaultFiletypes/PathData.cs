using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPLedit.NonDefaultFiletypes
{
    internal class PathData
    {
        public PathEntry[] PathEntries { get; private set; }

        public PathData(Timetable tt, Train train)
        {
            var path = train.GetPath();
            var arrDeps = train.GetArrDeps();

            int lastRoute = -1;
            int idx = -1;
            PathEntries = path.Select(sta =>
            {
                var hasArrDep = arrDeps.TryGetValue(sta, out var ardp);
                var next = (idx < path.Count - 1) ? path[idx + 1] : null;
                int route = next != null ? GetDirectlyConnectingRoute(tt, sta, next) : lastRoute;
                lastRoute = route;
                idx++;
                return new PathEntry(sta, ardp, route);
            }).ToArray();
        }

        private int GetDirectlyConnectingRoute(Timetable tt, Station sta1, Station sta2)
            => sta1.Routes.Intersect(sta2.Routes)
                .First(r => tt.RouteConnectsDirectly(r, sta1, sta2));

        //public bool IsDirectlyConnected(Station sta1, Station sta2)
        //{
        //    var path = GetRawPath().ToArray();
        //    for (int i = 0; i < path.Length; i++)
        //    {
        //        if (path[i] != sta1)
        //            continue;
        //        if (i > 0 && path[i - 1] == sta2)
        //            return true;
        //        if (i < path.Length - 1 && path[i + 1] == sta2)
        //            return true;
        //    }
        //    return false;
        //}

        public IEnumerable<Station> GetRawPath() => PathEntries.Select(e => e.Station);
    }

    internal class PathEntry
    {
        public PathEntry(Station station, ArrDep arrDep, int routeIndex)
        {
            Station = station;
            ArrDep = arrDep;
            RouteIndex = routeIndex;
        }

        public Station Station { get; private set; }

        public ArrDep ArrDep { get; private set; }

        public int RouteIndex { get; private set; }
    }
}

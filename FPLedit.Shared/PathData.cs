using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPLedit.Shared
{
    public class PathData
    {
        public PathEntry[] PathEntries { get; private set; }
        private Timetable tt;

        public PathData(Timetable tt, Train train)
        {
            this.tt = tt;
            var path = train.GetPath();
            var arrDeps = train.GetArrDeps();

            int lastRoute = -1;
            int idx = 0;
            PathEntries = path.Select(sta =>
            {
                var hasArrDep = arrDeps.TryGetValue(sta, out var ardp);
                var next = (idx < path.Count - 1) ? path[idx + 1] : null;
                int route = next != null ? tt.GetDirectlyConnectingRoute(sta, next) : lastRoute;
                lastRoute = route;
                idx++;
                return new PathEntry(sta, ardp, route);
            }).ToArray();
        }

        public Station NextStation(Station sta)
            => PathEntries.SkipWhile(pe => pe.Station != sta).Skip(1).FirstOrDefault()?.Station;

        public int GetExitRoute(Station sta)
        {
            if (sta == PathEntries.LastOrDefault()?.Station)
                return -1;
            var next = NextStation(sta);
            return tt.GetDirectlyConnectingRoute(sta, next);
        }

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

    public class PathEntry
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

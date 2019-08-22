using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPLedit.Shared
{
    public class PathData
    {
        protected PathEntry[] entries;
        public PathEntry[] PathEntries => entries;

        private readonly Timetable tt;

        public PathData(Timetable tt, IEnumerable<Station> path) : this(tt)
        {
            entries = Init(path.ToArray(), (s, r) => new PathEntry(s, r));
        }

        protected PathData(Timetable tt)
        {
            this.tt = tt;
        }

        protected T[] Init<T>(IReadOnlyList<Station> path, Func<Station, int, T> instanciator) where T : PathEntry
        {
            int lastRoute = -1;
            int idx = 0;
            return path.Select(sta =>
            {
                var next = (idx < path.Count - 1) ? path[idx + 1] : null;
                int route = next != null ? tt.GetDirectlyConnectingRoute(sta, next) : lastRoute;
                lastRoute = route;
                idx++;
                return instanciator(sta, route);
            }).ToArray();
        }

        public Station NextStation(Station sta)
            => entries.SkipWhile(pe => pe.Station != sta).Skip(1).FirstOrDefault()?.Station;

        public int GetExitRoute(Station sta)
        {
            if (sta == entries.LastOrDefault()?.Station)
                return -1;
            var next = NextStation(sta);
            return tt.GetDirectlyConnectingRoute(sta, next);
        }

        public bool ContainsStation(Station sta) => Array.IndexOf(PathEntries, sta) != -1;

        public bool IsDirectlyConnected(Station sta1, Station sta2)
        {
            var path = GetRawPath().ToArray();
            for (int i = 0; i < path.Length; i++)
            {
                if (path[i] != sta1)
                    continue;
                if (i > 0 && path[i - 1] == sta2)
                    return true;
                if (i < path.Length - 1 && path[i + 1] == sta2)
                    return true;
            }
            return false;
        }

        public IEnumerable<Station> GetRawPath() => entries.Select(e => e.Station);

        public static PathData Empty(Timetable tt) => new PathData(tt, Array.Empty<Station>());
    }

    public class TrainPathData : PathData
    {
        public new TrainPathEntry[] PathEntries => (TrainPathEntry[])entries;

        public TrainPathData(Timetable tt, Train train) : base(tt)
        {
            var path = train.GetPath().ToArray();
            var arrDeps = train.GetArrDeps();

            entries = Init(path, (s, r) =>
            {
                arrDeps.TryGetValue(s, out var ardp);
                return new TrainPathEntry(s, ardp, r);
            });
        }
    }

    public class PathEntry
    {
        public PathEntry(Station station, int routeIndex)
        {
            Station = station;
            RouteIndex = routeIndex;
        }

        public Station Station { get; private set; }

        public int RouteIndex { get; private set; }
    }

    public class TrainPathEntry : PathEntry
    {
        public TrainPathEntry(Station station, ArrDep arrDep, int routeIndex) : base(station, routeIndex)
        {
            ArrDep = arrDep;
        }

        public ArrDep ArrDep { get; private set; }
    }
}

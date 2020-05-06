using System;
using System.Collections.Generic;
using System.Linq;

namespace FPLedit.Shared
{
    /// <summary>
    /// Provides a convenient API to work with fixed paths.
    /// </summary>
    [Templating.TemplateSafe]
    public class PathData : ISortedStations
    {
        protected PathEntry[] entries;
        public PathEntry[] PathEntries => entries;

        private readonly Timetable tt;

        /// <summary>
        /// Creates a new PathData instance with the given stations.
        /// </summary>
        public PathData(Timetable tt, IEnumerable<Station> path) : this(tt)
        {
            entries = Init(path.ToArray(), (s, r) => new PathEntry(s, r));
        }

        protected PathData(Timetable tt)
        {
            this.tt = tt;
        }

        /// <summary>
        /// Initialization method. Can be used to implement custom flavoured PathData types.
        /// </summary>
        /// <param name="path">The stations of this path.</param>
        /// <param name="instanciator">A custom instanciator used to create the path entries.</param>
        /// <typeparam name="T">The type of entries used.</typeparam>
        /// <returns></returns>
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

        /// <summary>
        /// Returns the next station after the given station, fóllowing this path, or null.
        /// </summary>
        public Station NextStation(Station sta)
            => entries.SkipWhile(pe => pe.Station != sta).Skip(1).FirstOrDefault()?.Station;

        /// <summary>
        /// Get the route this path exits the given station on.
        /// </summary>
        public int GetExitRoute(Station sta)
        {
            if (sta == entries.LastOrDefault()?.Station)
                return -1;
            var next = NextStation(sta);
            return tt.GetDirectlyConnectingRoute(sta, next);
        }

        /// <summary>
        /// Returns whether this path contains the given station.
        /// </summary>
        public bool ContainsStation(Station sta) => Array.IndexOf(PathEntries, sta) != -1;

        /// <summary>
        /// Returns whether this path connects the two given stations directly.
        /// </summary>
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
        
        /// <summary>
        /// Returns +/-<paramref name="radius"/> stations around the given <paramref name="center"/> station, following
        /// the order of the current path.
        /// </summary>
        /// <remarks>
        /// The result will be silently truncated, if there are less than <paramref name="radius"/> stations on
        /// either side of the <paramref name="center"/> station.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">The radius was not positive.</exception>
        public Station[] GetSurroundingStations(Station center, int radius)
        {
            if (radius < 0)
                throw new ArgumentOutOfRangeException(nameof(radius));
            
            var stations = GetRawPath().ToArray();

            var centerIndex = Array.IndexOf(stations, center);
            if (centerIndex < 0)
                return Array.Empty<Station>(); // Not in the current path

            var leftIndex = Math.Max(centerIndex - radius, 0);
            var rightIndex = Math.Min(centerIndex + radius, stations.Length - 1);
            var length = rightIndex - leftIndex + 1;
            
            var array = new Station[length];
            Array.Copy(stations, array, length);
            return array;
        }

        /// <summary>
        /// Returns only the raw stations used in this path, without any additional information.
        /// </summary>
        public IEnumerable<Station> GetRawPath() => entries.Select(e => e.Station);

        /// <summary>
        /// Get an empty <see cref="PathData"/>-Instance.
        /// </summary>
        public static PathData Empty(Timetable tt) => new PathData(tt, Array.Empty<Station>());
    }
    
    /// <summary>
    /// Specific PathData, initialized with just a a train, containing the whole path &amp; time entries for this train.
    /// </summary>
    [Templating.TemplateSafe]
    public class TrainPathData : PathData
    {
        public new TrainPathEntry[] PathEntries => (TrainPathEntry[])entries;

        public TrainPathData(Timetable tt, ITrain train) : base(tt)
        {
            var path = train.GetPath().ToArray();
            var arrDeps = train.GetArrDepsUnsorted();

            entries = Init(path, (s, r) =>
            {
                arrDeps.TryGetValue(s, out var ardp);
                return new TrainPathEntry(s, ardp, r);
            });
        }
    }

    /// <summary>
    /// Entry of a default <see cref="PathData"/> structure. Represents one station on a specific route.
    /// </summary>
    [Templating.TemplateSafe]
    public class PathEntry
    {
        public PathEntry(Station station, int routeIndex)
        {
            Station = station;
            RouteIndex = routeIndex;
        }

        public Station Station { get; }

        public int RouteIndex { get; }
    }
    
    /// <summary>
    /// Entry of a <see cref="TrainPathData"/>. Represents one station on a specific route with attached time entry data.
    /// </summary>
    [Templating.TemplateSafe]
    public class TrainPathEntry : PathEntry
    {
        public TrainPathEntry(Station station, ArrDep arrDep, int routeIndex) : base(station, routeIndex)
        {
            ArrDep = arrDep;
        }

        public ArrDep ArrDep { get; }
    }
}

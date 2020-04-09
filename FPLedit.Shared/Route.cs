using System;
using System.Collections.Generic;
using System.Linq;

namespace FPLedit.Shared
{
    /// <summary>
    /// helper class (not part of the obecjt model), representing one of the routes the current timetable instance consists of.
    /// </summary>
    [Templating.TemplateSafe]
    public sealed class Route
    {
        private readonly Station[] stations;
        
        /// <summary>
        /// Unique identifier of this route.
        /// </summary>
        public int Index { get; }

        /// <summary>
        /// Ordered list of the stations on this route.
        /// </summary>
        public IList<Station> Stations => Array.AsReadOnly(stations);

        /// <summary>
        /// Returns the minimum kilometer position on this line (minimum chainage).
        /// </summary>
        public float MinPosition
            => Exists ? stations[0].Positions.GetPosition(Index) ?? 0f : 0f;

        /// <summary>
        /// Returns the maximum kilometer position on this line (maximum chainage).
        /// </summary>
        public float MaxPosition
            => Exists ? stations[stations.Length - 1].Positions.GetPosition(Index) ?? 0f : 0f;

        /// <summary>
        /// Returns whether this route is not empty.
        /// </summary>
        public bool Exists => stations.Length > 0;

        /// <summary>
        /// Create a new read-only instance with the given index an the given (unsorted) stations.
        /// </summary>
        internal Route(int index, IEnumerable<Station> stations)
        {
            Index = index;
            this.stations = stations.OrderBy(s => s.Positions.GetPosition(Index)).ToArray();
        }

        /// <summary>
        /// Returns a display name of the route.
        /// </summary>
        public string GetRouteName()
        {
            if (!Exists)
                return "<leer>";
            return stations[0].SName + " - " + stations[stations.Length - 1].SName; // this will always work, as stations.Length >= 1
        }

        /// <summary>
        /// Returns +/-<paramref name="radius"/> stations around the given <paramref name="center"/> station, following
        /// the chainage of the current route.
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
            var centerIndex = Array.IndexOf(stations, center);
            var leftIndex = Math.Max(centerIndex - radius, 0);
            var rightIndex = Math.Min(centerIndex + radius, stations.Length - 1);
            var length = rightIndex - leftIndex + 1;
            
            var array = new Station[length];
            Array.Copy(stations, array, length);
            return array;
        }
    }
}

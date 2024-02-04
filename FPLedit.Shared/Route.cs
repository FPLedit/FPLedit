using System;
using System.Collections.Generic;
using System.Linq;

namespace FPLedit.Shared;

/// <summary>
/// helper class (not part of the object model), representing one of the routes the current timetable instance consists of.
/// </summary>
[Templating.TemplateSafe]
public sealed class Route : ISortedStations
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
        => Exists ? stations[^1].Positions.GetPosition(Index) ?? 0f : 0f;

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
    /// Gets the index of the given station in the current route's ordered station list, or -1 if it is not part of this route.
    /// </summary>
    public int IndexOf(Station sta) => Array.IndexOf(stations, sta);

    /// <summary>
    /// Returns a display name of the route.
    /// </summary>
    public string GetRouteName(bool reverse = false)
    {
        if (!Exists)
            return T._("<leer>");
        return reverse
            ? stations[^1].SName + " - " + stations[ 0].SName
            : stations[ 0].SName + " - " + stations[^1].SName; // this will always work, as stations.Length >= 1
    }

    public PathData ToPathData(Timetable tt) => new PathData(this, tt);

    /// <summary>
    /// Returns +/-<paramref name="radius"/> stations around the given <paramref name="center"/> station, following
    /// the chainage of the current route.
    /// </summary>
    /// <remarks>
    /// <para>The result will be silently truncated, if there are less than <paramref name="radius"/> stations on
    /// either side of the <paramref name="center"/> station.</para>
    /// <para>If the station does not exist on the current route, an empty array will be returned.</para>
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">The radius was not positive.</exception>
    public Station[] GetSurroundingStations(Station center, int radius)
    {
        if (radius < 0)
            throw new ArgumentOutOfRangeException(nameof(radius));

        var centerIndex = Array.IndexOf(stations, center);
        if (centerIndex < 0)
            return Array.Empty<Station>(); // Not in the current route.

        var leftIndex = Math.Max(centerIndex - radius, 0);
        var rightIndex = Math.Min(centerIndex + radius, stations.Length - 1) + 1;

        return stations[leftIndex..rightIndex];
    }

    /// <summary>
    /// Returns only the raw stations used in this path, without any additional information.
    /// </summary>
    public IEnumerable<Station> GetRawPath() => Stations;
}
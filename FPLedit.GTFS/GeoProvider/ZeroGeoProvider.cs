using System;
using System.Collections.Generic;

namespace FPLedit.GTFS.GeoProvider;

public class ZeroGeoProvider : IGeoProvider
{
    private readonly List<string> missedQueries = new();

    public (float lat, float lon)? GetGeoPoint(string stationName)
    {
        missedQueries.Add(stationName);
        return null;
    }

    public (float lat, float lon)[] GetGeoLine(string routeName)
    {
        missedQueries.Add(routeName);
        return Array.Empty<(float lat, float lon)>();
    }

    public IEnumerable<string> GetMissedQueries() => missedQueries;
}
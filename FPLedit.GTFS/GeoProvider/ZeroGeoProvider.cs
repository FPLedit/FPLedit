using System;

namespace FPLedit.GTFS.GeoProvider;

public class ZeroGeoProvider : IGeoProvider
{
    public (float lat, float lon)? GetGeoPoint(string stationName) => null;
    public (float lat, float lon)[] GetGeoLine(string routeName) => Array.Empty<(float lat, float lon)>();
}
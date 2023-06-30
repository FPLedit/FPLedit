namespace FPLedit.GTFS.GeoProvider;

public interface IGeoProvider
{
    (float lat, float lon)? GetGeoPoint(string stationName);
    (float lat, float lon)[] GetGeoLine(string routeName);
}
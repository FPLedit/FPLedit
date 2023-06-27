namespace FPLedit.GTFS.GTFSLib;

public sealed class Stop
{
    [GtfsField("stop_id", GtfsType.Id)]
    public string StopId { get; init; }

    [GtfsField("stop_name", GtfsType.Text)]
    private string StopName { get; init; }

    [GtfsField("stop_lat", GtfsType.CoordLat)]
    public float StopLat { get; init; }
    [GtfsField("stop_lon", GtfsType.CoordLon)]
    public float StopLon { get; init; }

    [GtfsField("location_type", GtfsType.Enum)]
    public LocationType LocationType => LocationType.Station;

    [GtfsField("wheelchair_boarding", GtfsType.Enum, Optional = true)]
    public AccessibilityState WheelchairBoarding { get; init; }
}

public enum LocationType
{
    Station = 1, // Currently, only Station is supported
}
namespace FPLedit.GTFS.GTFSLib;

public sealed class Trip
{
    [GtfsField("route_id", GtfsType.Id, ForeignKey = "route_id")]
    public Route Route { get; init; }

    [GtfsField("trip_id", GtfsType.Id)]
    public string TripId { get; init; }

    [GtfsField("service_id", GtfsType.Id, ForeignKey = "service_id")]
    public Calendar Service { get; init; }
    
    [GtfsField("trip_short_name", GtfsType.Text, Optional = true)]
    public string TripShortName { get; init; }
    
    [GtfsField("direction_id", GtfsType.Enum, Optional = true)]
    public TripDirection DirectionId { get; init; }
    
    [GtfsField("bikes_allowed", GtfsType.Enum, Optional = true)]
    public AccessibilityState BikesAllowed { get; init; }
    
    [GtfsField("wheelchair_accessible", GtfsType.Enum, Optional = true)]
    public AccessibilityState WheelchairAccessible { get; init; }
    
    [GtfsField("shape_id", GtfsType.Id, ForeignKey = "shape_id", Optional = true)] // actually not optional, but we don't support continuos stops.
    public Shape Shape { get; init; }
}

public enum TripDirection
{
    Direction0 = 0,
    Direction1 = 1,
}

public enum AccessibilityState
{
    NotDefined = 0,
    Allowed = 1,
    NotAllowed = 2,
}
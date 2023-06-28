using FPLedit.Shared;

namespace FPLedit.GTFS.GTFSLib;

public sealed class Trip : IGtfsEntity
{
    [GtfsField("route_id", GtfsType.Id)] public Route Route { get; init; }

    [GtfsField("trip_id", GtfsType.Id)] public string TripId { get; init; }

    [GtfsField("service_id", GtfsType.Id)] public Calendar Service { get; init; }

    [GtfsField("trip_short_name", GtfsType.Text, Optional = true)]
    public string TripShortName { get; init; }

    [GtfsField("direction_id", GtfsType.Enum, Optional = true)]
    public TripDirection DirectionId { get; init; }

    [GtfsField("bikes_allowed", GtfsType.Enum, Optional = true)]
    public AccessibilityState BikesAllowed { get; set; }

    [GtfsField("wheelchair_accessible", GtfsType.Enum, Optional = true)]
    public AccessibilityState WheelchairAccessible { get; set; }

    [GtfsField("shape_id", GtfsType.Id, Optional = true)] // actually not optional, but we don't support continuos stops.
    public Shape Shape { get; set; }

    public static Trip FromTrain(Route route, Calendar service, ITrain train)
    {
        return new()
        {
            Route = route,
            TripId = GtfsField.ToId(train.TName),
            Service = service,
            TripShortName = train.TName,
            DirectionId = train.Direction == TrainDirection.ti ? TripDirection.Direction0 : TripDirection.Direction1,
        };
    }

    public string GetPkProperty() => nameof(TripId);
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
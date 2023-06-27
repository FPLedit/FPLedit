namespace FPLedit.GTFS.GTFSLib;

public sealed class Route
{
    [GtfsField("route_id", GtfsType.Id)]
    public string RouteId { get; init; }

    [GtfsField("route_short_name", GtfsType.Text, Optional = true)]
    public string RouteShortName { get; init; }
    [GtfsField("route_long_name", GtfsType.Text, Optional = true)]
    public string RouteLongName { get; init; }

    [GtfsField("route_long_name", GtfsType.Text)]
    public RouteType RouteType { get; init; }
}

public enum RouteType
{
    LightRail = 0,
    UndergroundRail = 1,
    Rail = 2,
    Bus = 3,
    Ferry = 4,
    CableTram = 5,
    AeriaLift = 6,
    Funicular = 7,
    Trolleybus = 11,
    Monorail = 12,
}
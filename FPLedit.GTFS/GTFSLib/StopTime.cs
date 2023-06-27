using FPLedit.Shared;

namespace FPLedit.GTFS.GTFSLib;

public sealed class StopTime
{
    [GtfsField("trip_id", GtfsType.Id, ForeignKey = "trip_id")]
    public Trip Trip { get; init; }

    [GtfsField("arrival_time", GtfsType.Time)]
    public TimeEntry ArrivalTime { get; init; }
    [GtfsField("departure_time", GtfsType.Time)]
    public TimeEntry DepartureTime { get; init; }
    
    [GtfsField("stop_id", GtfsType.Id, ForeignKey = "stop_id")]
    public Stop Stop { get; init; }
    
    [GtfsField("stop_sequence", GtfsType.UInt)]
    public uint StopSequence { get; init; }
    
    [GtfsField("pickup_type", GtfsType.Enum, Optional = true)]
    public PickupDropOffType PickupType { get; init; } = PickupDropOffType.Regular;
    [GtfsField("drop_off_type", GtfsType.Enum, Optional = true)]
    public PickupDropOffType DropOffType { get; init; } = PickupDropOffType.Regular;
}

public enum PickupDropOffType : int
{
    Regular = 0,
    None = 1,
    PhoneAgency = 2,
    CoordinateDriver = 3,
}
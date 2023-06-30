using System.Collections.Generic;
using System.Linq;
using FPLedit.Shared;

namespace FPLedit.GTFS.GTFSLib;

public sealed class StopTime : IGtfsEntity
{
    [GtfsField("trip_id", GtfsType.Id)]
    public Trip Trip { get; init; }

    [GtfsField("arrival_time", GtfsType.Time, Optional = true)] // not really optional, one of arrival and departure must be set.
    public TimeEntry? ArrivalTime { get; init; }
    [GtfsField("departure_time", GtfsType.Time, Optional = true)] // not really optional, one of arrival and departure must be set.
    public TimeEntry? DepartureTime { get; init; }

    [GtfsField("stop_id", GtfsType.Id)]
    public Stop Stop { get; init; }

    [GtfsField("stop_sequence", GtfsType.UInt)]
    public uint StopSequence { get; init; }

    [GtfsField("pickup_type", GtfsType.Enum, Optional = true)]
    public PickupDropOffType PickupType { get; init; } = PickupDropOffType.Regular;
    [GtfsField("drop_off_type", GtfsType.Enum, Optional = true)]
    public PickupDropOffType DropOffType { get; init; } = PickupDropOffType.Regular;

    public static StopTime FromArrDep(Trip t, ArrDep arrDep, bool isFirst, bool isLast, uint seq, Stop stop)
    {
        var pickupType = arrDep.RequestStop ? PickupDropOffType.CoordinateDriver : PickupDropOffType.Regular;
        return new()
        {
            Trip = t,
            ArrivalTime = !isFirst ? arrDep.Arrival : null,
            DepartureTime = !isLast ? arrDep.Departure : null,

            PickupType = !isLast ? pickupType : PickupDropOffType.None,
            DropOffType = !isFirst ? pickupType : PickupDropOffType.None,

            Stop = stop,
            StopSequence = seq,
        };
    }

    public static IEnumerable<StopTime> FromTrain(Trip t, ITrain train, Dictionary<IStation, Stop> stopMap)
    {
        var path = train.GetPath();
        uint seq = 0;
        var arrDeps = path.Select(s => (ArrDep: train.GetArrDep(s), Station: s)).Where(a => a.ArrDep.HasMinOneTimeSet).ToArray();
        foreach (var a in arrDeps)
        {
            yield return FromArrDep(t, a.ArrDep, seq == 0, seq == arrDeps.Length, seq + 1, stopMap[a.Station]);
            seq++;
        }
    }

    public string? GetPkProperty() => null;
}

public enum PickupDropOffType : int
{
    Regular = 0,
    None = 1,
    PhoneAgency = 2,
    CoordinateDriver = 3,
}
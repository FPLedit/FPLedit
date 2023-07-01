using System;
using FPLedit.Shared;

namespace FPLedit.GTFS.GTFSLib;

public interface ICalendar
{
    [GtfsField("service_id", GtfsType.Id)]
    public string? ServiceId { get; init; }

    public string GetTripIdSuffix();
}

public sealed class Calendar : IGtfsEntity, ICalendar
{
    [GtfsField("service_id", GtfsType.Id)]
    public string? ServiceId { get; init; }

    [GtfsField("monday", GtfsType.Bool)]
    public bool Monday { get; init; }
    [GtfsField("tuesday", GtfsType.Bool)]
    public bool Tuesday { get; init; }
    [GtfsField("wednesday", GtfsType.Bool)]
    public bool Wednesday { get; init; }
    [GtfsField("thursday", GtfsType.Bool)]
    public bool Thursday { get; init; }
    [GtfsField("friday", GtfsType.Bool)]
    public bool Friday { get; init; }
    [GtfsField("saturday", GtfsType.Bool)]
    public bool Saturday { get; init; }
    [GtfsField("sunday", GtfsType.Bool)]
    public bool Sunday { get; init; }

    [GtfsField("start_date", GtfsType.Date)]
    public DateOnly StartDate { get; init; }
    [GtfsField("end_date", GtfsType.Date)]
    public DateOnly EndDate { get; init; }

    public static Calendar FromTrain(ITrain train, DateOnly start, DateOnly end)
    {
        return new()
        {
            ServiceId = GtfsField.ToId(train.TName),
            Monday = train.Days[0],
            Tuesday = train.Days[1],
            Wednesday = train.Days[2],
            Thursday = train.Days[3],
            Friday = train.Days[4],
            Saturday = train.Days[5],
            Sunday = train.Days[6],
            StartDate = start,
            EndDate = end,
        };
    }

    public string? GetPkProperty() => nameof(ServiceId);
    public string GetTripIdSuffix() => "";
}

public sealed class CalendarDate : IGtfsEntity, ICalendar
{
    [GtfsField("service_id", GtfsType.Id)]
    public string? ServiceId { get; init; }

    [GtfsField("date", GtfsType.Date)]
    public DateOnly Date { get; init; }

    [GtfsField("exception_type", GtfsType.Enum)]
    public CalendarDateType ExceptionType { get; init; }

    public string? GetPkProperty() => null;
    public string GetTripIdSuffix() => "__" + Date.ToString("yyyyMMdd");
}

public enum CalendarDateType
{
    Added = 1,
    Removed = 2,
}
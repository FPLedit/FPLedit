namespace FPLedit.GTFS.GTFSLib;

public sealed class Calendar
{
    [GtfsField("service_id", GtfsType.Id)]
    public string Service { get; init; }

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
    public string StartDate { get; init; }
    [GtfsField("end_date", GtfsType.Date)]
    public string EndDate { get; init; }
}

public sealed class CalendarDate
{
    [GtfsField("service_id", GtfsType.Id, ForeignKey = "service_id")]
    public Calendar Service { get; init; }

    [GtfsField("date", GtfsType.Date)]
    public string Date { get; init; }

    [GtfsField("exception_type", GtfsType.Enum)]
    public CalendarDateType ExceptionType { get; init; }
}

public enum CalendarDateType
{
    Added = 1,
    Removed = 2,
}
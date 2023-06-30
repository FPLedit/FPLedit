namespace FPLedit.GTFS.GTFSLib;

// Simple model, agency_id is not set.
public sealed class Agency : IGtfsEntity
{
    [GtfsField("agency_name", GtfsType.Text)]
    public string AgencyName { get; init; }
    
    [GtfsField("agency_url", GtfsType.URL)]
    public string AgencyUrl { get; init; }
    
    [GtfsField("agency_timezone", GtfsType.Text /* not really */)]
    public string AgencyTimezone { get; init; }
    
    [GtfsField("agency_lang", GtfsType.Text /* not really */, Optional = true)]
    public string AgencyLang { get; init; }
    
    [GtfsField("agency_phone", GtfsType.Text /* not really */, Optional = true)]
    public string AgencyPhone { get; init; }
    
    [GtfsField("agency_email", GtfsType.Text /* not really */, Optional = true)]
    public string AgencyEmail { get; init; }
    
    [GtfsField("agency_fare_url", GtfsType.URL, Optional = true)]
    public string AgencyFareUrl { get; init; }

    public string? GetPkProperty() => null;
}
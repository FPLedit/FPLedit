using System;

namespace FPLedit.GTFS.GTFSLib;

[AttributeUsage(AttributeTargets.Property)]
public class GtfsField : Attribute
{
    public string FieldName { get; }
    public GtfsType Type { get; }
    public string ForeignKey { get; set; }
    public bool Optional { get; set; }

    public GtfsField(string fieldName, GtfsType type)
    {
        FieldName = fieldName;
        Type = type;
    }

    public void Validate()
    {
        if (ForeignKey != null && Type != GtfsType.Id)
            throw new Exception("GTFS Field with foreign key must be ID!");
    }
}

public enum GtfsType
{
    Id,
    Text,
    URL,
    Enum,
    Time,
    Date,
    UInt,
    Float,
    Bool, // originally an enum with 0=no, 1=yes
    
    CoordLat = Float,
    CoordLon = Float,
}
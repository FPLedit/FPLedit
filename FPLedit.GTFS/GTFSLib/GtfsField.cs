using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using FPLedit.Shared;

namespace FPLedit.GTFS.GTFSLib;

[AttributeUsage(AttributeTargets.Property)]
public sealed class GtfsField : Attribute
{
    public string FieldName { get; }
    public GtfsType Type { get; }
    public bool Optional { get; set; }

    public GtfsField(string fieldName, GtfsType type)
    {
        FieldName = fieldName;
        Type = type;
    }

    public string? GetValue(object x)
    {
        switch (Type)
        {
            case GtfsType.Text:
            case GtfsType.URL:
                if (x is null && Optional) return "";
                if (x is string s) return EscapeString(s); 
                throw new ArgumentException("wrong argument type for GTFS conversion");
            case GtfsType.UInt:
                if (x is uint u) return u.ToString();
                throw new ArgumentException("wrong argument type for GTFS conversion");
            case GtfsType.Float:
                if (x is float f) return f.ToString(CultureInfo.InvariantCulture);
                throw new ArgumentException("wrong argument type for GTFS conversion");
            case GtfsType.Bool:
                if (x is bool b) return (b ? 1 : 0).ToString();
                throw new ArgumentException("wrong argument type for GTFS conversion");
            case GtfsType.Enum:
                if (x is Enum e)
                {
                    var underlying = Enum.GetUnderlyingType(x.GetType());
                    var v = (int) Convert.ChangeType(e, underlying);
                    return v.ToString();
                }
                throw new ArgumentException("wrong argument type for GTFS conversion");
            case GtfsType.Time:
                if (x is null && Optional) return "";
                if (x is TimeEntry tm) return tm.ToTimeString().Replace(":", ""); //TODO: Handle decimals.
                throw new ArgumentException("wrong argument type for GTFS conversion");
            case GtfsType.Date:
                if (x is null && Optional) return "";
                if (x is DateOnly dt) return dt.ToString("YYYYMMDD");
                throw new ArgumentException("wrong argument type for GTFS conversion");
            case GtfsType.Id:
                if (x is IGtfsEntity ge)
                {
                    var prop = ge.GetType()!.GetProperty(ge.GetPkProperty(), BindingFlags.Instance | BindingFlags.Public);
                    if (prop == null || prop.PropertyType != typeof(string))
                        throw new ArgumentException("wrong foreign key type for GTFS conversion");
                    return EscapeString((string)prop.GetValue(ge));
                }

                if (x is string pk)
                    return EscapeString(pk);

                if (x == null && Optional)
                    return "";

                throw new ArgumentException("wrong argument type for GTFS conversion");
            default:
                throw new ArgumentException("undefined type conversion in GTFS field conversion");
        }
    }

    public static List<(string field, string value, bool optional)> GetValues(IGtfsEntity entry)
    {
        var dict = new List<(string field, string value, bool optional)>();
        var props = entry.GetType()!.GetProperties(BindingFlags.Instance | BindingFlags.Public);
        foreach (var prop in props)
        {
            var gtfsField = prop.GetCustomAttribute<GtfsField>();
            if (gtfsField == null)
                continue;

            dict.Add((gtfsField.FieldName, gtfsField.GetValue(prop.GetValue(entry)), gtfsField.Optional));
        }

        return dict;
    }

    private string EscapeString(string s)
    {
        if (!s.Contains('"') && !s.Contains(','))
            return s;
        return '"' + s.Replace("\"", "\"\"") + '"';
    }

    public static string ToId(string s) => s;
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
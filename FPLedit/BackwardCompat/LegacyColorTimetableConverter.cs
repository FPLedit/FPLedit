using System;
using System.Linq;
using FPLedit.Shared;
using FPLedit.Shared.Rendering;

namespace FPLedit.BackwardCompat;

internal static class LegacyColorTimetableConverter
{
    public static void ConvertAll(Timetable tt, TimetableVersion? forceVersion = null)
    {
        var timetableAttributes = new[] { "bgC", "hlC" };
        var stationAttributes = new[] { "cl" };
        var trainAttributes = new[] { "cl" };

        var version = forceVersion ?? tt.Version;

        ConvertElement(tt.XMLEntity, timetableAttributes, version);

        foreach (var sta in tt.Stations)
            ConvertElement(sta.XMLEntity, stationAttributes, version);

        foreach (var tra in tt.Trains)
            ConvertElement(tra.XMLEntity, trainAttributes, version);
    }

    private static void ConvertElement(XMLEntity xml, string[] attributesToConvert, TimetableVersion version)
    {
        foreach (var xe in xml.Attributes.ToArray())
        {
            if (attributesToConvert.Contains(xe.Key))
                xml.SetAttribute(xe.Key, ConvertColor(xe.Value, version));
        }
    }

    private static string ConvertColor(string value, TimetableVersion version)
    {
        var mcolor = ColorFormatter.FromString(value, null);
        if (mcolor != null)
            return ColorFormatter.ToString(mcolor, version == TimetableVersion.JTG2_x);
        throw new Exception("Found unknown color string " + value);
    }
}
using System;
using System.Collections.Generic;
using System.Linq;

namespace FPLedit.GTFS.GTFSLib;

public sealed class GtfsFile
{
    public Agency? Agency { get; set; }
    public Route? Route { get; set; }

    public List<Stop> Stops { get; } = new();
    public List<Calendar> Calendars { get; } = new();
    public List<CalendarDate> CalendarDates { get; } = new();
    public List<Trip> Trips { get; } = new();
    public List<StopTime> StopTimes { get; } = new();
    public List<Shape> Shapes { get; } = new();
    
    
    private string GetCsvString<T>(IEnumerable<T> entries) where T : IGtfsEntity
    {
        string[,] table = GetValues(entries);
        int j_max = table.GetLength(0);
        string result = "";
        for (var i = 0; i < table.GetLength(1); i++)
        for (var j = 0; j < j_max; j++)
            result += table[j, i] + (j == j_max - 1 ? '\n' : ',');
        return result;
    }

    private string[,] GetValues<T>(IEnumerable<T> entries) where T : IGtfsEntity
    {
        var table = new Dictionary<string, List<string?>>();
        var omits = new Dictionary<string, bool>();
        foreach (var entry in entries)
        {
            var values = GtfsField.GetValues(entry);
            foreach (var (field, value, optional) in values)
            {
                if (!table.ContainsKey(field)) table[field] = new List<string?>();
                if (!omits.ContainsKey(field)) omits[field] = true;

                table[field].Add(value);
                omits[field] &= (string.IsNullOrEmpty(value) && optional);
            }
        }

        int length = -1;
        foreach (var field in table.Keys.ToArray())
        {
            if (omits.TryGetValue(field, out var omit) && omit)
            {
                table.Remove(field);
                continue;
            }

            if (length == -1)
                length = table[field].Count;
            if (length != table[field].Count)
                throw new Exception("GTFS lengths do not match");
        }

        var resultTable = new string[table.Keys.Count, length + 1];
        int j = 0;
        foreach (var kvp in table)
            resultTable[j++, 0] = kvp.Key;

        for (int i = 0; i < length; i++)
        {
            j = 0;
            foreach (var kvp in table)
                resultTable[j++, i + 1] = kvp.Value[i] ?? "";
        }

        return resultTable;
    }

    public Dictionary<string, string> GetFiles()
    {
        if (Agency == null || Route == null)
            throw new Exception("Agency or Route instance not set!");

        var files = new Dictionary<string, string>();
        files["agencies.txt"] = GetCsvString(new[] { Agency });
        files["routes.txt"] = GetCsvString(new[] { Route });
        files["stops.txt"] = GetCsvString(Stops);
        files["trips.txt"] = GetCsvString(Trips);
        files["stop_times.txt"] = GetCsvString(StopTimes);
        files["calendars.txt"] = GetCsvString(Calendars);
        if (CalendarDates.Any())
            files["calendar_dates.txt"] = GetCsvString(CalendarDates);
        if (Shapes.Any())
            files["shapes.txt"] = GetCsvString(Shapes);
        return files;
    }
}

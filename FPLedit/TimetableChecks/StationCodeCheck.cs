using FPLedit.Shared;
using System.Collections.Generic;
using System.Linq;

namespace FPLedit.TimetableChecks;

internal sealed class StationCodeCheck : ITimetableCheck
{
    public string Display => T._("Ungültige Stationsabkürzungen");

    public IEnumerable<string> Check(Timetable tt)
    {
        var codes = tt.Stations
            .Select(s => s.StationCode)
            .Where(s => s != "")
            .GroupBy(n => n)
            .Where(c => c.Count() > 1)
            .Select(g => g.Key);
        if (codes.Any())
            yield return T._("Betriebsstellen-Abkürzungen {0} sind mehr als ein mal vorhanden!", string.Join(", ", codes));
    }
}
using FPLedit.Shared;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FPLedit.TimetableChecks;

internal sealed class TrainsTrackCheck : ITimetableCheck
{
    public string Display => T._("Ungültige Gleisangaben");

    public IEnumerable<string> Check(Timetable tt)
    {
        var result = new ConcurrentBag<string>();
        var stations = tt.Stations.ToDictionary(s => s, s => s.Tracks.Select(t => t.Name));
        Parallel.ForEach(tt.Trains, tra =>
        {
            var arrdeps = tra.GetArrDepsUnsorted();
            foreach (var ardep in arrdeps)
            {
                var tracks = stations[ardep.Key].ToList();

                if (!string.IsNullOrEmpty(ardep.Value.ArrivalTrack) && !tracks.Contains(ardep.Value.ArrivalTrack))
                    result.Add(T._("Ungültiges Ankunftsgleis: Zug {0} / Station {1} / Gleis \"{2}\" nicht gefunden.", tra.TName, ardep.Key.SName, ardep.Value.ArrivalTrack));
                if (!string.IsNullOrEmpty(ardep.Value.DepartureTrack) && !tracks.Contains(ardep.Value.DepartureTrack))
                    result.Add(T._("Ungültiges Abfahrtsgleis: Zug {0} / Station {1} / Gleis \"{2}\" nicht gefunden.", tra.TName, ardep.Key.SName, ardep.Value.DepartureTrack));
                foreach (var shunt in ardep.Value.ShuntMoves)
                {
                    if ((!string.IsNullOrEmpty(shunt.SourceTrack) && !tracks.Contains(shunt.SourceTrack)) || (!string.IsNullOrEmpty(shunt.TargetTrack) && !tracks.Contains(shunt.TargetTrack)))
                        result.Add(T._("Ungültiges Rangiergleis: Zug {0} / Station {1} / Gleis nicht gefunden.", tra.TName, ardep.Key.SName));
                }
            }
        });
        return result;
    }
}
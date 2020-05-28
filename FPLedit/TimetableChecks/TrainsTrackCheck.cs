using FPLedit.Shared;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FPLedit.TimetableChecks
{
    internal sealed class TrainsTrackCheck : ITimetableCheck
    {
        public string Display => "Ungültige Gleisangaben";

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
                        result.Add($"Ungültiges Ankunftsgleis: Zug {tra.TName} / Station {ardep.Key.SName} / Gleis \"{ardep.Value.ArrivalTrack}\" nicht gefunden.");
                    if (!string.IsNullOrEmpty(ardep.Value.DepartureTrack) && !tracks.Contains(ardep.Value.DepartureTrack))
                        result.Add($"Ungültiges Abfahrtsgleis: Zug {tra.TName} / Station {ardep.Key.SName} / Gleis \"{ardep.Value.DepartureTrack}\" nicht gefunden.");
                    foreach (var shunt in ardep.Value.ShuntMoves)
                    {
                        if ((!string.IsNullOrEmpty(shunt.SourceTrack) && !tracks.Contains(shunt.SourceTrack)) || (!string.IsNullOrEmpty(shunt.TargetTrack) && !tracks.Contains(shunt.TargetTrack)))
                            result.Add($"Ungültiges Rangiergleis: Zug {tra.TName} / Station {ardep.Key.SName} / Gleis \"{ardep.Value.DepartureTrack}\" nicht gefunden.");
                    }
                }
            });
            return result;
        }
    }
}

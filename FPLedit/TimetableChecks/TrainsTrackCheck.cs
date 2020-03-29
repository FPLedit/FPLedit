using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPLedit.TimetableChecks
{
    internal sealed class TrainsTrackCheck : ITimetableCheck
    {
        public string Display => "Ungültige Gleisangaben";

        public IEnumerable<string> Check(Timetable tt)
        {
            var stations = tt.Stations.ToDictionary(s => s, s => s.Tracks.Select(t => t.Name));
            foreach (var tra in tt.Trains)
            {
                var arrdeps = tra.GetArrDepsUnsorted();
                foreach (var ardep in arrdeps)
                {
                    var tracks = stations[ardep.Key].ToList();
                    
                    if (!string.IsNullOrEmpty(ardep.Value.ArrivalTrack) && !tracks.Contains(ardep.Value.ArrivalTrack))
                        yield return $"Ungültiges Ankunftsgleis: Zug {tra.TName} / Station {ardep.Key.SName} / Gleis \"{ardep.Value.ArrivalTrack}\" nicht gefunden.";
                    if (!string.IsNullOrEmpty(ardep.Value.DepartureTrack) && !tracks.Contains(ardep.Value.DepartureTrack))
                        yield return $"Ungültiges Abfahrtsgleis: Zug {tra.TName} / Station {ardep.Key.SName} / Gleis \"{ardep.Value.DepartureTrack}\" nicht gefunden.";
                    foreach (var shunt in ardep.Value.ShuntMoves)
                    {
                        if ((!string.IsNullOrEmpty(shunt.SourceTrack) && !tracks.Contains(shunt.SourceTrack)) || (!string.IsNullOrEmpty(shunt.TargetTrack) && !tracks.Contains(shunt.TargetTrack)))
                            yield return $"Ungültiges Rangiergleis: Zug {tra.TName} / Station {ardep.Key.SName} / Gleis \"{ardep.Value.DepartureTrack}\" nicht gefunden.";
                    }
                }
            }
        }
    }
}

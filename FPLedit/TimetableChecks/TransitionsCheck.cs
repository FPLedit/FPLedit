using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPLedit.TimetableChecks
{
    internal class TransitionsCheck : ITimetableCheck
    {
        public void Check(Timetable tt, ILog log)
        {
            foreach (var tra in tt.Transitions)
            {
                var first = tt.GetTrainById(tra.First);
                var next = tt.GetTrainById(tra.Next);

                if (first == null || next == null)
                    return;

                var lastStaOfFirst = GetSortedStations(first)?.LastOrDefault();
                var firstStaOfNext = GetSortedStations(next)?.FirstOrDefault();

                if (lastStaOfFirst == null || firstStaOfNext == null)
                    continue;

                if (lastStaOfFirst != firstStaOfNext)
                    log.Warning($"Der Folgezug {next.TName} beginnt an einer anderen Station, als die, an der vorherige Zug {first.TName} endet.");

                if (first.GetArrDep(lastStaOfFirst).FirstSetTime > next.GetArrDep(firstStaOfNext).FirstSetTime)
                    log.Warning($"Der Abfahrtszeit des Folgezuges {next.TName} ist früher als die Ankunftszeit des vorherigen Zuges {first.TName}.");
            }
        }

        private IEnumerable<Station> GetSortedStations(Train train)
        {
            var path = train.GetPath();
            var arrdeps = train.GetArrDeps();
            foreach (var sta in path)
            {
                if (arrdeps[sta].HasMinOneTimeSet)
                    yield return sta;
            }
        }
    }
}

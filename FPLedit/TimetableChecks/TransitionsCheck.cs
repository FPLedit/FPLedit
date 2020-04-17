using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPLedit.TimetableChecks
{
    internal sealed class TransitionsCheck : ITimetableCheck
    {
        public string Display => "Ungültige Folgezüge";

        public IEnumerable<string> Check(Timetable tt)
        {
            foreach (var tra in tt.Transitions)
            {
                var first = tt.GetTrainById(tra.First);
                var next = tt.GetTrainById(tra.Next);

                if (first == null || next == null)
                   continue;

                var lastStaOfFirst = GetSortedStations(first)?.LastOrDefault();
                var firstStaOfNext = GetSortedStations(next)?.FirstOrDefault();

                if (lastStaOfFirst == null || firstStaOfNext == null)
                    continue;

                if (lastStaOfFirst != firstStaOfNext)
                    yield return $"Der Folgezug {next.TName} beginnt an einer anderen Station, als die, an der vorherige Zug {first.TName} endet.";

                if (first.GetArrDep(lastStaOfFirst).FirstSetTime > next.GetArrDep(firstStaOfNext).FirstSetTime)
                    yield return $"Der Abfahrtszeit des Folgezuges {next.TName} ist früher als die Ankunftszeit des vorherigen Zuges {first.TName}.";
            }
        }

        private IEnumerable<Station> GetSortedStations(ITrain train)
        {
            var path = train.GetPath();
            var arrdeps = train.GetArrDepsUnsorted();
            foreach (var sta in path)
            {
                if (arrdeps[sta].HasMinOneTimeSet)
                    yield return sta;
            }
        }
    }
}

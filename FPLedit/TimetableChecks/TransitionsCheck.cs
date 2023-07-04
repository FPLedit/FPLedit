using System.Collections.Concurrent;
using FPLedit.Shared;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FPLedit.TimetableChecks
{
    internal sealed class TransitionsCheck : ITimetableCheck
    {
        public string Display => T._("Ungültige Folgezüge");

        public IEnumerable<string> Check(Timetable tt)
        {
            var result = new ConcurrentBag<string>();
            Parallel.ForEach(tt.Transitions, tra =>
            {
                var first = tt.GetTrainByQualifiedId(tra.First);
                var next = tt.GetTrainByQualifiedId(tra.Next);

                if (first == null || next == null)
                    return;

                var lastStaOfFirst = GetSortedStations(first)?.LastOrDefault();
                var firstStaOfNext = GetSortedStations(next)?.FirstOrDefault();

                if (lastStaOfFirst == null || firstStaOfNext == null)
                    return;

                if (lastStaOfFirst != firstStaOfNext)
                    result.Add(T._("Der Folgezug {0} beginnt an einer anderen Station, als die, an der vorherige Zug {1} endet.", next.TName, first.TName));
                if (first.GetArrDep(lastStaOfFirst).FirstSetTime > next.GetArrDep(firstStaOfNext).FirstSetTime)
                    result.Add(T._("Der Abfahrtszeit des Folgezuges {0} ist früher als die Ankunftszeit des vorherigen Zuges {1}.", next.TName, first.TName));
            });
            return result;
        }

        private IEnumerable<Station>? GetSortedStations(ITrain train)
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

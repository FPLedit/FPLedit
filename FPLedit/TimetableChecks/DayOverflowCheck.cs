using FPLedit.Shared;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FPLedit.TimetableChecks
{
    internal sealed class DayOverflowCheck : ITimetableCheck
    {
        public string Display => LocalizationHelper._("Zugverkehr über Mitternacht");

        public IEnumerable<string> Check(Timetable tt)
        {
            var result = new ConcurrentBag<string>();
            Parallel.ForEach(tt.Trains, train =>
            {
                var arrdeps = new TrainPathData(train.ParentTimetable, train);
                TimeEntry last = default;
                foreach (var arrdep in arrdeps.PathEntries)
                {
                    if (arrdep.ArrDep == null)
                        continue;

                    if (arrdep.ArrDep.HasMinOneTimeSet && arrdep.ArrDep.FirstSetTime < last)
                    {
                        result.Add(LocalizationHelper._("Der Zug {0} verkehrt über Mitternacht hinweg!", train.TName));
                        return;
                    }

                    last = arrdep.ArrDep.Departure == default ? arrdep.ArrDep.Arrival : arrdep.ArrDep.Departure;
                }
            });
            return result;
        }
    }
}

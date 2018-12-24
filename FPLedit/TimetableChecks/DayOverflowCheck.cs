using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPLedit.TimetableChecks
{
    internal class DayOverflowCheck : ITimetableCheck
    {
        public void Check(Timetable tt, ILog log)
        {
            foreach (var train in tt.Trains)
            {
                var arrdeps = GetSortedArrDeps(train);
                TimeSpan last = default;
                bool hasOverflow = false;
                foreach (var arrdep in arrdeps)
                {
                    if (arrdep.FirstSetTime < last)
                        hasOverflow = true;
                    last = arrdep.Departure == default ? arrdep.Arrival : arrdep.Departure;
                }

                if (hasOverflow)
                    log.Warning($"Der Zug {train.TName} verkehrt über Mitternacht hinweg!");
            }
        }

        private IEnumerable<ArrDep> GetSortedArrDeps(Train train)
        {
            var path = train.GetPath();
            var arrdeps = train.GetArrDeps();
            foreach (var sta in path)
            {
                if (arrdeps[sta].HasMinOneTimeSet)
                    yield return arrdeps[sta];
            }
        }
    }
}

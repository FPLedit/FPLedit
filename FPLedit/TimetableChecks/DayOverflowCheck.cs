using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPLedit.TimetableChecks
{
    internal sealed class DayOverflowCheck : ITimetableCheck
    {
        public string Display => "Zugverkehr über Mitternacht";

        public IEnumerable<string> Check(Timetable tt)
        {
            foreach (var train in tt.Trains)
            {
                var arrdeps = new TrainPathData(train.ParentTimetable, train);
                TimeEntry last = default;
                bool hasOverflow = false;
                foreach (var arrdep in arrdeps.PathEntries)
                {
                    if (arrdep.ArrDep == null)
                        continue;

                    if (arrdep.ArrDep.HasMinOneTimeSet && arrdep.ArrDep.FirstSetTime < last)
                        hasOverflow = true;
                    last = arrdep.ArrDep.Departure == default ?
                        arrdep.ArrDep.Arrival :
                        arrdep.ArrDep.Departure;
                }

                if (hasOverflow)
                    yield return $"Der Zug {train.TName} verkehrt über Mitternacht hinweg!";
            }
        }
    }
}

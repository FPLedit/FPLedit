using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPLedit.TimetableChecks
{
    internal class TimetableCheckRunner
    {
        public TimetableCheckRunner(IInfo info)
        {
            var checks = info.GetRegistered<ITimetableCheck>();
            info.FileStateChanged += (s, e) =>
            {
                if (info.Timetable == null)
                    return;

                foreach (var check in checks)
                    check.Check(info.Timetable, info.Logger);
            };
        }
    }
}

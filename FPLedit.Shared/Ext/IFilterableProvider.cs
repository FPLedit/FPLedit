using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FPLedit.Shared
{
    public interface IFilterableProvider
    {
        string DisplayName { get; }

        void SaveFilter(Timetable tt, List<FilterRule> stationRules, List<FilterRule> trainRules);

        List<FilterRule> LoadTrainRules(Timetable tt);

        List<FilterRule> LoadStationRules(Timetable tt);
    }
}

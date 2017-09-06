using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FPLedit.Shared.Ui
{
    public interface IFilterableUi
    {
        string DisplayName { get; }

        void SaveFilter(List<FilterRule> stationRules, List<FilterRule> trainRules);
    }
}

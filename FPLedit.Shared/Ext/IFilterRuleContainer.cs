using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FPLedit.Shared
{
    /// <summary>
    /// This component is used to provide filter rules for a specific template type to be used within the main application.
    /// Inheritors should not implement any additional logivc than loading/saving.
    /// </summary>
    /// <remarks>See <see cref="DefaultImplementations.DefaultFilterRuleContainer"/> for a default implementation.</remarks>
    public interface IFilterRuleContainer : IRegistrableComponent
    {
        /// <summary>
        /// Display name used in filter form.
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// Action that is triggered when the given filter rules should be commited to the Timetable.
        /// </summary>
        void SaveFilter(Timetable tt, List<FilterRule> stationRules, List<FilterRule> trainRules);

        /// <summary>
        /// Function invoked to retrieve the curretn train rules from the current Timetable.
        /// </summary>
        List<FilterRule> LoadTrainRules(Timetable tt);
        
        /// <summary>
        /// Function invoked to retrieve the current station rules from the current Timetable.
        /// </summary>
        List<FilterRule> LoadStationRules(Timetable tt);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using FPLedit.Shared.Helpers;

namespace FPLedit.Shared.DefaultImplementations
{
    public abstract class BaseFilterRuleContainer
    {
        private static readonly EscapeSplitHelper escape = new EscapeSplitHelper('|');

        protected abstract IPatternSource? GetProvider(Timetable tt);
        protected abstract IPatternSource CreateProvider(Timetable tt);

        public IEnumerable<FilterRule> LoadStationRules(Timetable tt) => escape.SplitEscaped(GetProvider(tt)?.StationPatterns ?? "").Select(s => new FilterRule(s));

        public IEnumerable<FilterRule> LoadTrainRules(Timetable tt) => escape.SplitEscaped(GetProvider(tt)?.TrainPatterns ?? "").Select(s => new FilterRule(s));

        private string Serialize(IEnumerable<FilterRule> rules)
        {
            var patterns = rules.Select(r => r.Pattern);
            return escape.JoinEscaped(patterns);
        }

        public void SaveFilter(Timetable tt, IEnumerable<FilterRule> newStationRules, IEnumerable<FilterRule> newTrainRules)
        {
            IPatternSource attrs = GetProvider(tt) ?? CreateProvider(tt);
            attrs.TrainPatterns = Serialize(newTrainRules);
            attrs.StationPatterns = Serialize(newStationRules);
        }
    }

    public sealed class DefaultFilterRuleContainer : BaseFilterRuleContainer, IFilterRuleContainer
    {
        private readonly Func<Timetable, IPatternSource?> getProvider;
        private readonly Func<Timetable, IPatternSource> createProvider;
        public string DisplayName { get; }
        
        public DefaultFilterRuleContainer(string displayName, Func<Timetable, IPatternSource?> getProvider, Func<Timetable,IPatternSource> createProvider)
        {
            this.getProvider = getProvider;
            this.createProvider = createProvider;
            DisplayName = displayName;
        }
        
        protected override IPatternSource? GetProvider(Timetable tt) => getProvider(tt);

        protected override IPatternSource CreateProvider(Timetable tt) => createProvider(tt);
    }
}
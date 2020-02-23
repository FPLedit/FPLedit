using System;
using System.Collections.Generic;
using System.Linq;

namespace FPLedit.Shared.DefaultImplementations
{
    public abstract class BaseFilterRuleContainer
    {
        private List<FilterRule> trainRules, stationRules;

        private void Init(Timetable tt)
        {
            var attrs = GetProvider(tt);
            if (attrs != null)
            {
                trainRules = attrs.TrainPatterns.Split('|').Where(p => p != "").Select(p => new FilterRule(p)).ToList();
                stationRules = attrs.StationPatterns.Split('|').Where(p => p != "").Select(p => new FilterRule(p)).ToList();
            }
            else
            {
                attrs = CreateProvider(tt);

                trainRules = new List<FilterRule>();
                stationRules = new List<FilterRule>();
            }
        }

        protected abstract IPatternSource GetProvider(Timetable tt);
        
        protected abstract IPatternSource CreateProvider(Timetable tt);

        public List<FilterRule> LoadStationRules(Timetable tt)
        {
            Init(tt);
            return stationRules;
        }

        public List<FilterRule> LoadTrainRules(Timetable tt)
        {
            Init(tt);
            return trainRules;
        }
        
        public void SaveFilter(Timetable tt, List<FilterRule> stationRules, List<FilterRule> trainRules)
        {
            IPatternSource attrs = GetProvider(tt);
            attrs.TrainPatterns = string.Join("|", trainRules.Select(r => r.Pattern));
            attrs.StationPatterns = string.Join("|", stationRules.Select(r => r.Pattern));
        }
    }

    public sealed class DefaultFilterRuleContainer : BaseFilterRuleContainer, IFilterRuleContainer
    {
        private readonly Func<Timetable, IPatternSource> getProvider, createProvider;
        public string DisplayName { get; }
        
        public DefaultFilterRuleContainer(string displayName, Func<Timetable, IPatternSource> getProvider, Func<Timetable,IPatternSource> createProvider)
        {
            this.getProvider = getProvider;
            this.createProvider = createProvider;
            DisplayName = displayName;
        }
        
        protected override IPatternSource GetProvider(Timetable tt) => getProvider(tt);

        protected override IPatternSource CreateProvider(Timetable tt) => createProvider(tt);
    }
}
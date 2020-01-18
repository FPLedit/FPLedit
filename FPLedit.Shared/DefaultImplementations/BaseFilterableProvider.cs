using System;
using System.Collections.Generic;
using System.Linq;

namespace FPLedit.Shared.DefaultImplementations
{
    public abstract class BaseFilterableProvider
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

        protected abstract IPatternProvider GetProvider(Timetable tt);
        
        protected abstract IPatternProvider CreateProvider(Timetable tt);

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
            IPatternProvider attrs = GetProvider(tt);
            attrs.TrainPatterns = string.Join("|", trainRules.Select(r => r.Pattern));
            attrs.StationPatterns = string.Join("|", stationRules.Select(r => r.Pattern));
        }
    }

    public sealed class BasicFilterableProvider : BaseFilterableProvider, IFilterableProvider
    {
        private readonly Func<Timetable, IPatternProvider> getProvider, createProvider;
        public string DisplayName { get; }
        
        public BasicFilterableProvider(string displayName, Func<Timetable, IPatternProvider> getProvider, Func<Timetable,IPatternProvider> createProvider)
        {
            this.getProvider = getProvider;
            this.createProvider = createProvider;
            DisplayName = displayName;
        }
        
        protected override IPatternProvider GetProvider(Timetable tt) => getProvider(tt);

        protected override IPatternProvider CreateProvider(Timetable tt) => createProvider(tt);
    }
}
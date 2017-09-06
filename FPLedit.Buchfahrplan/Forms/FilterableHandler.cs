using FPLedit.Shared.Ui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FPLedit.Shared;
using FPLedit.Buchfahrplan.Model;

namespace FPLedit.Buchfahrplan.Forms
{
    public class FilterableHandler : IFilterableUi
    {
        public string DisplayName => "Buchfahrplan";

        private List<FilterRule> trainRules, stationRules;

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

        private void Init(Timetable tt)
        {
            var attrs = BfplAttrs.GetAttrs(tt);
            if (attrs != null)
            {
                trainRules = attrs.TrainPatterns.Split('|').Where(p => p != "").Select(p => new FilterRule(p)).ToList();
                stationRules = attrs.StationPatterns.Split('|').Where(p => p != "").Select(p => new FilterRule(p)).ToList();
            }
            else
            {
                attrs = new BfplAttrs(tt);
                tt.Children.Add(attrs.XMLEntity);

                trainRules = new List<FilterRule>();
                stationRules = new List<FilterRule>();
            }
        }

        public void SaveFilter(Timetable tt, List<FilterRule> stationRules, List<FilterRule> trainRules)
        {
            var attrs = BfplAttrs.GetAttrs(tt);
            attrs.TrainPatterns = string.Join("|", trainRules.Select(r => r.Pattern));
            attrs.StationPatterns = string.Join("|", stationRules.Select(r => r.Pattern));
        }
    }
}

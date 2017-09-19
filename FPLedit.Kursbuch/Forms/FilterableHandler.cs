using FPLedit.Shared.Ui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FPLedit.Shared;
using FPLedit.Kursbuch.Model;

namespace FPLedit.Kursbuch.Forms
{
    public class FilterableHandler : IFilterableUi
    {
        public string DisplayName => "Kursbuch";

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
            var attrs = KfplAttrs.GetAttrs(tt);
            if (attrs != null)
            {
                trainRules = attrs.TrainPatterns.Split('|').Where(p => p != "").Select(p => new FilterRule(p)).ToList();
                stationRules = attrs.StationPatterns.Split('|').Where(p => p != "").Select(p => new FilterRule(p)).ToList();
            }
            else
            {
                attrs = new KfplAttrs(tt);
                tt.Children.Add(attrs.XMLEntity);

                trainRules = new List<FilterRule>();
                stationRules = new List<FilterRule>();
            }
        }

        public void SaveFilter(Timetable tt, List<FilterRule> stationRules, List<FilterRule> trainRules)
        {
            var attrs = KfplAttrs.GetAttrs(tt);
            attrs.TrainPatterns = string.Join("|", trainRules.Select(r => r.Pattern));
            attrs.StationPatterns = string.Join("|", stationRules.Select(r => r.Pattern));
        }
    }
}

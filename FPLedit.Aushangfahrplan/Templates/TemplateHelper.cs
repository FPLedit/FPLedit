using FPLedit.Aushangfahrplan.Model;
using FPLedit.Shared;
using System;
using System.Linq;

namespace FPLedit.Aushangfahrplan.Templates
{
    public class TemplateHelper
    {
        public Timetable TT { get; private set; }

        private FilterRule[] trules, srules;

        public TemplateHelper(Timetable tt)
        {
            TT = tt;

            var filterable = new Forms.FilterableHandler();

            trules = filterable.LoadTrainRules(tt).ToArray();
            srules = filterable.LoadTrainRules(tt).ToArray();
        }

        public Station[] GetStations()
        {
            return TT.Stations
                .Where(s => srules.All(r => !r.Matches(s)))
                .Where(s => GetTrains(TrainDirection.ta, s).Length > 0
                    || GetTrains(TrainDirection.ti, s).Length > 0)
                .ToArray();
        }

        public Train[] GetTrains(TrainDirection dir, Station sta)
        {
            return TT.Trains.Where(t => t.Direction == dir)
                .Where(t => t.GetArrDep(sta).Departure != default(TimeSpan))
                .Where(t => trules.All(r => !r.Matches(t)))
                .ToArray();
        }
    }
}
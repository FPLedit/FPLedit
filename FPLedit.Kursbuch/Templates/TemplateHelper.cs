using FPLedit.Shared;
using System;
using System.Linq;

namespace FPLedit.Kursbuch.Templates
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
            srules = filterable.LoadStationRules(tt).ToArray();
        }

        public Station[] GetStations(TrainDirection dir)
        {
            return TT.GetStationsOrderedByDirection(dir)
                .Where(s => srules.All(r => !r.Matches(s)))
                .ToArray();
        }

        public Train[] GetTrains(TrainDirection dir)
        {
            return TT.Trains.Where(t => t.Direction == dir)
                .Where(t => trules.All(r => !r.Matches(t)))
                .ToArray();
        }
    }
}
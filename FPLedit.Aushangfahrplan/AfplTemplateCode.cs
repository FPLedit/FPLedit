using FPLedit.AushangfahrplanExport.Model;
using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FPLedit.AushangfahrplanExport
{
    partial class AfplTemplate
    {
        private Timetable tt;
        private Rule[] trules, srules;

        public AfplTemplate(Timetable tt)
        {
            this.tt = tt;
            trules = new Rule[0];
            var attrs = AfplAttrs.GetAttrs(tt);
            if (attrs != null)
            {
                trules = attrs.TrainPatterns.Split('|')
                    .Where(p => p != "")
                    .Select(p => new Rule(p)).ToArray();

                srules = attrs.StationPatterns.Split('|')
                    .Where(p => p != "")
                    .Select(p => new Rule(p)).ToArray();
            }
        }

        private Station[] GetStations()
        {
            return tt.Stations
                .Where(s => srules.All(r => !r.Matches(s)))
                .ToArray();
        }

        private Train[] GetTrains(TrainDirection dir, Station sta)
        {
            return tt.Trains.Where(t => t.Direction == dir)
                .Where(t => t.GetArrDep(sta).Departure != default(TimeSpan))
                .Where(t => trules.All(r => !r.Matches(t)))
                .ToArray();
        }

        private string GetTimeString(TimeSpan t)
            => t.Hours.ToString() + "<sup>" + t.Minutes.ToString("00") + "</sup>";

        private string TimeString(Train[] trains, Station sta, int i)
            => trains.Count() > i ? GetTimeString(trains[i].GetArrDep(sta).Departure) : "";

        private string NameString(Train[] trains, int i)
            => trains.Count() > i ? trains[i].TName : "";
    }
}

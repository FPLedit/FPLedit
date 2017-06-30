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

        public AfplTemplate(Timetable tt)
        {
            this.tt = tt;
        }

        private string GetTimeString(TimeSpan t)
        {
            return t.Hours.ToString()+"<sup>" + t.Minutes.ToString("00")+"</sup>";
        }

        private Train[] GetTrainsByDir(TrainDirection dir, Station sta)
        {
            return tt.Trains.Where(t => t.Direction == dir)
                .Where(t => t.GetArrDep(sta).Departure != default(TimeSpan))
                .ToArray();
        }

        private string TimeString(Train[] trains, Station sta, int i)
            => trains.Count() > i ? GetTimeString(trains[i].GetArrDep(sta).Departure) : "";

        private string NameString(Train[] trains, int i)
            => trains.Count() > i ? trains[i].TName : "";
    }
}

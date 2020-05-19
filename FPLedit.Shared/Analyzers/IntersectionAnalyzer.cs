using System;
using System.Collections.Generic;
using System.Linq;

namespace FPLedit.Shared.Analyzers
{
    /// <summary>
    /// Provides helper methods for analysing the interaction of one train with other trains at a given station.
    /// </summary>
    public sealed class IntersectionAnalyzer
    {
        private readonly Timetable tt;

        /// <summary>
        /// Creates a new intersection analyzer for the given timetable.
        /// </summary>
        public IntersectionAnalyzer(Timetable tt)
        {
            this.tt = tt;
        }

        /// <summary>
        /// Finds all trains that are crossing with the given train at the given station.
        /// </summary>
        public IEnumerable<ITrain> CrossingAtStation(ITrain train, Station station) => IntersectTrainsAtStations(train, station, true);

        /// <summary>
        /// Finds all trains that overtake the given train at the given station.
        /// </summary>
        public IEnumerable<ITrain> OvertakeAtStation(ITrain train, Station station) => IntersectTrainsAtStations(train, station, false);

        /// <summary>
        /// Returns information about whether the given train has to stop before a German railway signal Ne1 (Trapeztafel),
        /// before proceeding into the station; or if any other crossing train has to.
        /// </summary>
        public TrapezEntry TrapezAtStation(ITrain train, Station station)
        {
            var its = IntersectTrainsAtStations(train, station, true);

            var days = Days.None;
            var stoppingTrains = new List<ITrain>();

            foreach (var it in its)
            {
                days = days.Union(it.Days);
                
                if (it.GetArrDep(station).TrapeztafelHalt)
                    stoppingTrains.Add(it);
            }

            if (!its.Any()) // We had no intersecting train, so we should return the trains days itself.
                days = Days.All;

            return new TrapezEntry(train.GetArrDep(station).TrapeztafelHalt, stoppingTrains.ToArray(), train.Days.IntersectingDays(days));
        }

        private IEnumerable<ITrain> IntersectTrainsAtStations(ITrain ot, Station s, bool crossing)
        {
            var probeTrainStart = ot.GetArrDep(s).Arrival;
            var probeTrainEnd = ot.GetArrDep(s).Departure;

            if (probeTrainStart == TimeEntry.Zero || probeTrainEnd == TimeEntry.Zero)
                yield break;

            // Prepare matching criteria for trains collection
            Func<ITrain, bool> pred;
            if (tt.Type == TimetableType.Linear)
            {
                pred = t => t.Direction == ot.Direction; // Overtaking
                if (crossing)
                    pred = t => t.Direction != ot.Direction; // Crossing
            }
            else
            {
                var surrounding = new TrainPathData(tt, ot).GetSurroundingStations(s, 1);
                pred = t =>
                {
                    var p = new TrainPathData(tt, t).GetSurroundingStations(s, 1);
                    var x = new int[p.Length];
                    for (int i = 0; i < p.Length; i++)
                        x[i] = Array.IndexOf(surrounding, p[i]);
                    
                    // We have only one station in common (the center station), this is definitely a crossing (from another route)
                    if (x.Count(i => i >= 0) < 2)
                        return crossing;
                    
                    var compare = x.FirstOrDefault().CompareTo(x.LastOrDefault());
                    return crossing ? compare > 0 : compare < 0;
                };
            }

            foreach (var train in tt.Trains.Where(pred))
            {
                if (train == ot || !train.GetPath().Contains(s))
                    continue;

                var ardp = train.GetArrDep(s);
                var curTrainStart = ardp.Arrival;
                var curTrainEnd = ardp.Departure;

                if (curTrainStart == TimeEntry.Zero || curTrainEnd == TimeEntry.Zero)
                    continue;

                var st = probeTrainStart < curTrainStart ? curTrainStart : probeTrainStart;
                var en = probeTrainEnd < curTrainEnd ? probeTrainEnd : curTrainEnd;
                var isCrossing = st <= en;

                if (isCrossing && ot.Days.IsIntersecting(train.Days))
                    yield return train;
            }
        }
    }

    public sealed class TrapezEntry
    {
        public TrapezEntry(bool isStopping, ITrain[] intersectingTrainsStopping, Days stopDays)
        {
            IsStopping = isStopping;
            IntersectingTrainsStopping = intersectingTrainsStopping;
            StopDays = stopDays;
        }

        public bool IsStopping { get; }
        public ITrain[] IntersectingTrainsStopping { get; }
        public Days StopDays { get; }
    }
}
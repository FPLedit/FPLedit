using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using FPLedit.Shared.Helpers;

namespace FPLedit.Editor.Trains
{
    internal sealed class TrainEditHelper
    {
        private Train CopyTrain(Train orig, int offsetMin, string name, bool copyAll)
        {
            var t = new Train(orig.Direction, orig.ParentTimetable)
            {
                TName = name,
                Comment = orig.Comment,
                Days = orig.Days,
                Last = orig.Last,
                Locomotive = orig.Locomotive,
                Mbr = orig.Mbr,
            };

            if (copyAll)
                foreach (var attr in orig.Attributes)
                    if (t.GetAttribute<string>(attr.Key) == null)
                        t.SetAttribute(attr.Key, attr.Value);

            var path = orig.GetPath();

            if (orig.ParentTimetable.Type == TimetableType.Network)
                t.AddAllArrDeps(path);
            else
                t.AddLinearArrDeps();

            InternalCopyArrDeps(orig, t, offsetMin);

            return t;
        }

        public void FillTrain(Train orig, Train target, int offsetMin)
        {
            if (orig.ParentTimetable.Type == TimetableType.Network)
                throw new TimetableTypeNotSupportedException(TimetableType.Network, "fill trains");
            if (orig.Direction != target.Direction || orig == target)
                throw new InvalidOperationException("Invalid call to FillTrain: Either the trains have not the same direction or are equal.");

            InternalCopyArrDeps(orig, target, offsetMin);
        }

        public IEnumerable<ITrain> FillCandidates(ITrain tra)
            => tra.ParentTimetable.Trains.Where(t => t.Direction == tra.Direction && t != tra);

        public void MoveTrain(Train t, int offsetMin)
            => InternalCopyArrDeps(t, t, offsetMin);

        private void InternalCopyArrDeps(Train source, Train destination, int offsetMin)
        {
            var offset = new TimeEntry(0, offsetMin);
            var path = source.GetPath();

            foreach (var sta in path)
            {
                var ardp = source.GetArrDep(sta).Copy();
                if (sta != path.First() && ardp.Arrival != default)
                    ardp.Arrival = ardp.Arrival.Add(offset);
                if (sta != path.Last() && ardp.Departure != default)
                    ardp.Departure = ardp.Departure.Add(offset);
                destination.GetArrDep(sta).ApplyCopy(ardp);
            }
        }

        public Train[] CopyTrainMultiple(Train orig, int offsetMin, string name, bool copyAll, int count, int numberAdd)
        {
            if (count < 0)
                throw new ArgumentException("Value must be greater than or equal to zero", nameof(count));
            
            var ret = new Train[count];

            var np = new TrainNameParts(name);
            var start = np.Number;

            for (int i = 0; i < count; i++)
            {
                var n = np.BaseName + (start + numberAdd * (i + 1)).ToString();
                ret[i] = CopyTrain(orig, offsetMin * (i + 1), n, copyAll);
            }

            return ret;
        }
        
        public void LinkTrainMultiple(Train orig, int offsetMin, int diffMin, int count, ITrainLinkNameCalculator tnc)
        {
            if (count < 0)
                throw new ArgumentException("Value must be greater than or equal to zero", nameof(count));

            var link = new TrainLink(orig, count)
            {
                TimeDifference = diffMin, 
                TimeOffset = offsetMin, 
                TrainNamingScheme = tnc,
            };
            orig.AddLink(link);

            for (int i = 0; i < count; i++)
            {
                var linkedTrain = new LinkedTrain(link, i);
                orig.ParentTimetable!.AddTrain(linkedTrain);
            }
        }

        public void SortTrainsAllStations(Timetable tt, TrainDirection dir, bool topDown)
        {
            if (tt.Type != TimetableType.Linear)
                throw new TimetableTypeNotSupportedException(tt.Type, "Sorting at all stations");
            
            var stations = tt.GetLinearStationsOrderedByDirection(dir);
            if (!topDown)
                stations.Reverse();

            foreach (var sta in stations)
                SortTrainsAtStation(tt, dir, sta);
        }

        public void SortTrainsName(Timetable tt, TrainDirection dir, bool excludePrefix)
        {
            if (excludePrefix)
                SortTrainsName(tt, dir, false);

            ITrain[] Trains() => tt.Trains.Where(t => t.Direction == dir).ToArray();

            TrainNameParts NameSelector(ITrain train) => new TrainNameParts(train.TName);
            bool StringComparer(TrainNameParts cur, TrainNameParts next) => cur.CompareTo(next, excludePrefix);

            InternalSort(tt, Trains, NameSelector, StringComparer);
        }

        public void SortTrainsAtStation(Timetable tt, TrainDirection dir, Station sta)
        {
            ITrain[] Trains() => tt.Trains.Where(t => t.Direction == dir)
                .Where(t => t.GetPath().Contains(sta)).ToArray();

            TimeEntry TimeSelector(ITrain train) => train.GetArrDep(sta).FirstSetTime;
            bool TimeComparer(TimeEntry cur, TimeEntry next) => (cur != default) && (next != default) && (cur > next);

            InternalSort(tt, Trains, TimeSelector, TimeComparer);
        }

        private void InternalSort<TCompare>(Timetable tt, Func<ITrain[]> trains, Func<ITrain, TCompare> selector, Func<TCompare, TCompare, bool> comparer)
        {
            var t = trains();
            for (int n = t.Length; n > 1; n--)
            {
                // Bubblesort
                for (int i = 0; i < n - 1; i++)
                {
                    var train = t[i];

                    if (i + 1 == t.Length)
                        break; // Wir sind durch

                    var next = t.ElementAt(i + 1);

                    var firstVal = selector(train);
                    var secondVal = selector(next);

                    if (comparer(firstVal, secondVal))
                    {
                        tt._InternalSwapTrainOrder(train, next);
                        t = trains();
                    }
                }
            }
        }
    }
}

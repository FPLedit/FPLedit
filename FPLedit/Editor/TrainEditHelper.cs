using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace FPLedit.Editor
{
    internal class TrainEditHelper
    {
        public Train CopyTrain(Train orig, int offsetMin, string name, bool copyAll)
        {
            var offset = new TimeSpan(0, offsetMin, 0);

            var t = new Train(orig.Direction, orig._parent)
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

            if (orig._parent.Type == TimetableType.Network)
                t.AddAllArrDeps(path);
            else
                foreach (var sta in orig._parent.Stations)
                    t.AddArrDep(sta, new ArrDep(), Timetable.LINEAR_ROUTE_ID);

            foreach (var sta in path)
            {
                var ardp = orig.GetArrDep(sta);
                if (sta != path.First() && ardp.Arrival != default)
                    ardp.Arrival = ardp.Arrival.Add(offset);
                if (sta != path.Last() && ardp.Departure != default)
                    ardp.Departure = ardp.Departure.Add(offset);
                t.SetArrDep(sta, ardp);
            }

            return t;
        }

        public void MoveTrain(Train t, int offsetMin)
        {
            var offset = new TimeSpan(0, offsetMin, 0);

            var path = t.GetPath();

            foreach (var sta in path)
            {
                var ardp = t.GetArrDep(sta);
                if (sta != path.First() && ardp.Arrival != default)
                    ardp.Arrival = ardp.Arrival.Add(offset);
                if (sta != path.Last() && ardp.Departure != default)
                    ardp.Departure = ardp.Departure.Add(offset);
                t.SetArrDep(sta, ardp);
            }
        }

        public Train[] CopyTrainMultiple(Train orig, int offsetMin, string name, bool copyAll, int count, int numberAdd)
        {
            var ret = new Train[count];

            var start = RemoveNamePrefix(name, out string baseName);

            for (int i = 0; i < count; i++)
            {
                var n = baseName + (start + numberAdd * (i + 1)).ToString();
                ret[i] = CopyTrain(orig, offsetMin * (i + 1), n, copyAll);
            }

            return ret;
        }

        public void SortTrainsAllStations(Timetable tt, TrainDirection dir, bool topDown)
        {
            var stations = tt.GetStationsOrderedByDirection(dir);
            if (!topDown)
                stations.Reverse();

            foreach (var sta in stations)
                SortTrainsAtStation(tt, dir, sta);
        }

        public void SortTrainsName(Timetable tt, TrainDirection dir, bool excludePrefix)
        {
            var trains = tt.Trains.ToList();

            string NameSelector(Train train) => excludePrefix ? RemoveNamePrefix(train.TName, out var _).ToString() : train.TName;
            bool StringComparer(string cur, string next) => cur.CompareTo(next) > 0;

            InternalSort(tt, trains, NameSelector, StringComparer);
        }

        public void SortTrainsAtStation(Timetable tt, TrainDirection dir, Station sta)
        {
            var trains = tt.Trains.Where(t => t.Direction == dir)
                .Where(t => t.GetPath().Contains(sta))
                .ToList();

            TimeSpan TimeSelector(Train train) => train.GetArrDep(sta).FirstSetTime;
            bool TimeComparer(TimeSpan cur, TimeSpan next) => (cur != default) && (next != default) && (cur > next);

            InternalSort(tt, trains, TimeSelector, TimeComparer);
        }

        public void InternalSort<TCompare>(Timetable tt, List<Train> trains, Func<Train, TCompare> selector, Func<TCompare, TCompare, bool> comparer)
        {
            // Bubblesort
            foreach (var train in trains)
            {
                var idx = trains.IndexOf(train);

                if (idx + 1 == trains.Count)
                    break; // Wir sind durch

                var next = trains.ElementAt(idx + 1);

                var firstVal = selector(train);
                var secondVal = selector(next);

                if (comparer(firstVal, secondVal))
                    SwapTrains(tt, train, next);
            }
        }

        private void SwapTrains(Timetable tt, Train t1, Train t2)
        {
            var tElm = tt.XMLEntity.Children.FirstOrDefault(x => x.XName == "trains");

            var idx = tt.Trains.IndexOf(t1);
            var xidx = tElm.Children.IndexOf(t1.XMLEntity);

            tt.Trains.Remove(t2);
            tElm.Children.Remove(t2.XMLEntity);

            tt.Trains.Insert(idx, t2);
            tElm.Children.Insert(xidx, t2.XMLEntity);
        }


        private int RemoveNamePrefix(string name, out string prefix)
        {
            var nameBase = name.Trim();
            var last = nameBase.ToCharArray().LastOrDefault();
            var num = "";
            while (char.IsDigit(last))
            {
                num = last + num;
                nameBase = nameBase.Substring(0, nameBase.Length - 1);
                last = nameBase.ToCharArray().LastOrDefault();
            }

            int.TryParse(num, out int start); // Startnummer

            prefix = nameBase;
            return start;
        }
    }
}

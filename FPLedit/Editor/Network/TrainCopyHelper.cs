using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FPLedit.Editor.Network
{
    internal class TrainCopyHelper
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
            t.AddAllArrDeps(path);

            foreach (var sta in path)
            {
                var ardp = orig.GetArrDep(sta);
                if (sta != path.First() && ardp.Arrival != default(TimeSpan))
                    ardp.Arrival = ardp.Arrival.Add(offset);
                if (sta != path.Last() && ardp.Departure != default(TimeSpan))
                    ardp.Departure = ardp.Departure.Add(offset);
                t.SetArrDep(sta, ardp);
            }

            return t;
        }
    }
}

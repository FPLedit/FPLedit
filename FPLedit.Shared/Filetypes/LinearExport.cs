using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPLedit.Shared.Filetypes
{
    public class LinearExport : IExport
    {
        public string Filter => "Fahrplan Dateien (*.fpl)|*.fpl";

        public bool Export(Timetable tt, string filename, IInfo info)
        {
            if (tt.Type == TimetableType.Linear)
                throw new Exception("Der Fahrplan ist bereits ein Linear-Fahrplan");
            if (tt.GetRoutes().Count() > 1)
                throw new Exception("Der Fahrplan hat mehr als eine Strecke");

            var clone = tt.Clone();
            var ntt = new Timetable(TimetableType.Linear);

            foreach (var attr in clone.Attributes)
                if (ntt.GetAttribute<string>(attr.Key) == null)
                    ntt.SetAttribute(attr.Key, attr.Value);

            foreach (var sta in clone.Stations)
            {
                var km_old = sta.GetAttribute("km", "").Split(':');
                sta.SetAttribute("km", km_old[1]);
                sta.RemoveAttribute("fpl-rt");
                sta.RemoveAttribute("fpl-pos");
                ntt.AddStation(sta, Timetable.LINEAR_ROUTE_ID);
            }

            var sortedStations = tt.GetRoutes()[0].GetOrderedStations();

            foreach (var orig in tt.Trains)
            {
                var path = orig.GetPath();

                var sta1 = path.FirstOrDefault();
                var sta2 = path.LastOrDefault();

                var dir = TrainDirection.ti;
                if (sta1 != sta2)
                {
                    if (sortedStations.IndexOf(sta1) > sortedStations.IndexOf(sta2))
                        dir = TrainDirection.ta;
                }
                else if (sortedStations.IndexOf(sta1) == sortedStations.Count - 1)
                    dir = TrainDirection.ta;

                var stas = sortedStations;
                if (dir == TrainDirection.ta)
                    stas = stas.Reverse<Station>().ToList();

                var ntra = new Train(dir, ntt);

                foreach (var attr in orig.Attributes)
                    if (ntra.GetAttribute<string>(attr.Key) == null)
                        ntra.SetAttribute(attr.Key, attr.Value);

                ntt.AddTrain(ntra);

                foreach (var sta in path)
                {
                    var ardp = orig.GetArrDep(sta);
                    var nsta = ntt.Stations[tt.Stations.IndexOf(sta)];
                    ntra.SetArrDep(nsta, ardp);
                }
            }

            return new XMLExport().Export(ntt, filename, info);
        }
    }
}

using FPLedit.Shared;
using FPLedit.Shared.Filetypes;
using FPLedit.Shared.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FPLedit.NonDefaultFiletypes
{
    internal class LinearExport : IExport
    {
        public string Filter => "Fahrplan Dateien (*.fpl)|*.fpl";

        public bool Export(Timetable tt, string filename, IInfo info)
        {
            if (tt.Type == TimetableType.Linear)
                throw new Exception("Der Fahrplan ist bereits ein Linear-Fahrplan");
            if (tt.GetRoutes().Count() > 1)
                throw new Exception("Der Fahrplan hat mehr als eine Strecke");

            var clone = tt.Clone();

            Dictionary<Train, TrainData> trainPaths = new Dictionary<Train, TrainData>();
            foreach (var orig in clone.Trains)
                trainPaths[orig] = new TrainData(orig);

            clone.SetAttribute("version", Timetable.DefaultLinearVersion.ToNumberString());

            foreach (var sta in clone.Stations)
            {
                var km_old = sta.GetAttribute("km", "").Split(':');
                sta.RemoveAttribute("km");
                var pos = km_old[1];

                if (clone.Version == TimetableVersion.JTG2_x)
                    sta.SetAttribute("km", pos);
                else // jTG 3.0
                {
                    sta.SetAttribute("kml", pos);
                    sta.SetAttribute("kmr", pos);
                }

                sta.RemoveAttribute("fpl-rt");
                sta.RemoveAttribute("fpl-pos");
                sta.RemoveAttribute("fpl-id");
            }

            var sortedStations = clone.GetRoutes()[0].GetOrderedStations();

            foreach (var t in clone.Trains)
            {
                var data = trainPaths[t];

                var sta1 = data.Path.FirstOrDefault();
                var sta2 = data.Path.LastOrDefault();

                var dir = TrainDirection.ti;
                if (sta1 != sta2)
                {
                    if (sortedStations.IndexOf(sta1) > sortedStations.IndexOf(sta2))
                        dir = TrainDirection.ta;
                }
                else if (sortedStations.IndexOf(sta1) == sortedStations.Count - 1)
                    dir = TrainDirection.ta;

                t.XMLEntity.XName = dir.ToString();

                t.Children.Clear();
                t.AddLinearArrDeps();

                foreach (var sta in data.Path)
                {
                    if (data.ArrDeps.ContainsKey(sta))
                        t.SetArrDep(sta, data.ArrDeps[sta]);
                }
            }

            ColorTimetableConverter.ConvertAll(clone);

            return new XMLExport().Export(clone, filename, info);
        }
    }
}

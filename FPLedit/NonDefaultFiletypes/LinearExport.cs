using FPLedit.Shared;
using FPLedit.Shared.Filetypes;
using FPLedit.Shared.Rendering;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FPLedit.NonDefaultFiletypes
{
    internal sealed class LinearExport : BaseConverterFileType, IExport
    {
        public string Filter => "Fahrplan Dateien (*.fpl)|*.fpl";

        public bool Export(Timetable tt, Stream stream, IPluginInterface pluginInterface, string[] flags = null)
        {
            if (tt.Type == TimetableType.Linear)
                throw new TimetableTypeNotSupportedException(TimetableType.Linear, "convert to linear");
            if (tt.GetRoutes().Count() != 1)
                throw new NotSupportedException("Der Fahrplan hat mehr als eine oder keine Strecke");

            var clone = tt.Clone();

            var trainPaths = new Dictionary<Train, TrainPathData>();
            foreach (var orig in clone.Trains)
                trainPaths[orig] = new TrainPathData(clone, orig);

            var route = clone.GetRoutes().FirstOrDefault().Index;

            foreach (var sta in clone.Stations)
            {
                ConvertStationNetToLin(sta, route, Timetable.DefaultLinearVersion);

                sta.RemoveAttribute("fpl-rt");
                sta.RemoveAttribute("fpl-pos");
                sta.RemoveAttribute("fpl-id");
            }

            var actions = pluginInterface.GetRegistered<ITimetableTypeChangeAction>();
            foreach (var action in actions)
                action.ToLinear(clone);

            clone.SetAttribute("version", Timetable.DefaultLinearVersion.ToNumberString());

            var sortedStations = clone.GetRoutes()[Timetable.LINEAR_ROUTE_ID].Stations;

            foreach (var t in clone.Trains)
            {
                var data = trainPaths[t];

                var sta1 = data.PathEntries.FirstOrDefault()?.Station;
                var sta2 = data.PathEntries.LastOrDefault()?.Station;

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

                foreach (var sta in data.PathEntries)
                {
                    if (sta.ArrDep != null)
                        t.GetArrDep(sta.Station).ApplyCopy(sta.ArrDep);
                }
            }

            ColorTimetableConverter.ConvertAll(clone);

            return new XMLExport().Export(clone, stream, pluginInterface);
        }
    }
}

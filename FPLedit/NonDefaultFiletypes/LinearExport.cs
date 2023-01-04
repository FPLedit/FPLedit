#nullable enable
using FPLedit.Shared;
using FPLedit.Shared.Filetypes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FPLedit.NonDefaultFiletypes
{
    internal sealed class LinearExport : BaseConverterFileType, IExport
    {
        public string Filter => T._("Fahrplan Dateien (*.fpl)|*.fpl");

        public bool Export(Timetable tt, Stream stream, IReducedPluginInterface pluginInterface, string[]? flags = null)
        {
            if (tt.Type == TimetableType.Linear)
                throw new TimetableTypeNotSupportedException(TimetableType.Linear, "convert to linear");
            if (tt.GetRoutes().Length != 1)
                throw new NotSupportedException(T._("Der Fahrplan hat mehr als eine oder keine Strecke"));
            
            if (tt.Version.CompareTo(TimetableVersion.Extended_FPL2) >= 0 && Timetable.DefaultLinearVersion.CompareTo(TimetableVersion.JTG3_3) < 0)
                throw new NotSupportedException(T._("Eine Fahrplandatei der Version >= 101 kann nicht als lineare Datei <= 012 exportiert werden!"));

            var clone = tt.Clone();

            var trainPaths = new Dictionary<ITrain, TrainPathData>();
            foreach (var orig in clone.Trains)
                trainPaths[orig] = new TrainPathData(clone, orig);

            var route = clone.GetRoutes().Single().Index;

            foreach (var sta in clone.Stations)
            {
                ConvertStationNetToLin(sta, route);

                sta.RemoveAttribute("fpl-rt");
                sta.RemoveAttribute("fpl-pos");
                sta.RemoveAttribute("fpl-id");
            }

            var actions = pluginInterface.GetRegistered<ITimetableTypeChangeAction>();
            foreach (var action in actions)
                action.ToLinear(clone);

            clone.SetVersion(Timetable.DefaultLinearVersion);

            var sortedStations = clone.GetRoutes()[Timetable.LINEAR_ROUTE_ID].Stations;

            foreach (var t in clone.Trains)
            {
                var data = trainPaths[t];

                var sta1 = data.PathEntries.FirstOrDefault()?.Station!;
                var sta2 = data.PathEntries.LastOrDefault()?.Station!;

                var dir = TrainDirection.ti;
                if (sta1 != sta2)
                {
                    if (sortedStations.IndexOf(sta1) > sortedStations.IndexOf(sta2))
                        dir = TrainDirection.ta;
                }
                else if (sortedStations.IndexOf(sta1) == sortedStations.Count - 1)
                    dir = TrainDirection.ta;

                t.XMLEntity.XName = dir.ToString();
                
                if (t is not IWritableTrain wt)
                    continue;

                wt.Children.Clear(); // Clear all existing arrdeps...
                wt.AddLinearArrDeps(); // ...and re-add all linear ones.

                foreach (var sta in data.PathEntries)
                {
                    if (sta.ArrDep != null)
                        t.GetArrDep(sta.Station).ApplyCopy(sta.ArrDep);
                }
            }

            return new XMLExport().Export(clone, stream, pluginInterface);
        }
    }
}

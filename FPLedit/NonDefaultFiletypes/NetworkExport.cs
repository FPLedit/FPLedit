using FPLedit.Shared;
using FPLedit.Shared.Filetypes;
using FPLedit.Shared.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FPLedit.NonDefaultFiletypes
{
    internal class NetworkExport : BaseConverterFileType, IExport
    {
        public string Filter => "Fahrplan Dateien (*.fpl)|*.fpl";

        public bool Export(Timetable tt, string filename, IInfo info)
        {
            if (tt.Type == TimetableType.Network)
                throw new Exception("Der Fahrplan ist bereits ein Netzwerk-Fahrplan");

            var clone = tt.Clone();
            var old_version = clone.GetAttribute("version", "");

            Dictionary<Train, TrainData> trainPaths = new Dictionary<Train, TrainData>();
            foreach (var orig in clone.Trains)
                trainPaths[orig] = new TrainData(orig);

            var rt = Timetable.LINEAR_ROUTE_ID.ToString();
            var id = 0;
            var y = 0;
            foreach (var sta in clone.Stations)
            {
                ConvertStationLinToNet(sta);

                sta.SetAttribute("fpl-rt", rt);
                sta.SetAttribute("fpl-pos", (y += 40).ToString() + ";0");
                sta.SetAttribute("fpl-id", id++.ToString());
            }

            clone.SetAttribute("version", TimetableVersion.Extended_FPL.ToNumberString());

            foreach (var orig in clone.Trains)
            {
                var data = trainPaths[orig];

                orig.Children.Clear();
                orig.AddAllArrDeps(data.Path);
                orig.XMLEntity.XName = "tr";

                foreach (var sta in data.Path)
                {
                    if (data.ArrDeps.ContainsKey(sta))
                        orig.SetArrDep(sta, data.ArrDeps[sta]);
                }
            }

            ColorTimetableConverter.ConvertAll(clone);

            return new XMLExport().Export(clone, filename, info);
        }
    }
}

using FPLedit.Shared;
using FPLedit.Shared.Filetypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPLedit.NonDefaultFiletypes
{
    internal class UpgradeJTG3Export : IExport
    {
        public string Filter => "Fahrplan Dateien (*.fpl)|*.fpl";

        public bool Export(Timetable tt, string filename, IInfo info)
        {
            if (tt.Version != TimetableVersion.JTG2_x)
                throw new Exception("Nur mit jTrainGraph 2.x erstellte Fahrpläne können aktualisiert werden.");

            var clone = tt.Clone();
            clone.SetAttribute("version", "009");

            var shv = clone.GetAttribute<bool>("shV");
            clone.SetAttribute("shV", shv ? "1" : "0");

            // km -> kml/kmr
            foreach (var sta in clone.Stations)
            {
                var km_old = sta.GetAttribute("km", "");
                sta.SetAttribute("kml", km_old);
                sta.SetAttribute("kmr", km_old);
                sta.RemoveAttribute("km");
            }

            // Allocate train ids
            var nextId = clone.Trains.Max(t => t.Id);
            foreach (var orig in clone.Trains)
            {
                if (orig.Id == -1)
                    orig.Id = ++nextId;
            }

            //TODO: Farben konvertieren: c(r,g,b) -> #RRGGBB

            return new XMLExport().Export(clone, filename, info);
        }
    }
}

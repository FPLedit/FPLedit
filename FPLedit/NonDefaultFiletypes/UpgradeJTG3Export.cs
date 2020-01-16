using FPLedit.Shared;
using FPLedit.Shared.Filetypes;
using FPLedit.Shared.Rendering;
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

        public bool Export(Timetable tt, string filename, IPluginInterface pluginInterface, string[] flags = null)
        {
            if (tt.Version.Compare(TimetableVersion.JTG3_1) >= 0)
                throw new Exception("Nur mit jTrainGraph 2.x oder 3.0x erstellte Fahrpläne können aktualisiert werden.");

            var origVersion = tt.Version;

            var clone = tt.Clone();
            clone.SetAttribute("version", "010");

            if (origVersion == TimetableVersion.JTG2_x)
            {
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
                var nextId = clone.Trains.DefaultIfEmpty().Max(t => t?.Id) ?? 0;
                foreach (var orig in clone.Trains)
                {
                    if (orig.Id == -1)
                        orig.Id = ++nextId;
                }

            }

            ColorTimetableConverter.ConvertAll(clone);

            return new XMLExport().Export(clone, filename, pluginInterface);
        }
    }
}

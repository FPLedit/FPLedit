using FPLedit.Shared;
using FPLedit.Shared.Filetypes;
using FPLedit.Shared.Rendering;
using System;
using System.IO;
using System.Linq;

namespace FPLedit.NonDefaultFiletypes
{
    internal sealed class UpgradeExport : IExport
    {
        public string Filter => "Fahrplan Dateien (*.fpl)|*.fpl";

        public bool Export(Timetable tt, Stream stream, IReducedPluginInterface pluginInterface, string[] flags = null)
        {
            if (tt.Version.Compare(TimetableVersion.JTG3_2) >= 0)
                throw new Exception("Nur mit jTrainGraph 2.x, 3.0x oder 3.1x erstellte Fahrpläne können aktualisiert werden.");

            var origVersion = tt.Version;

            var xclone = tt.XMLEntity.XClone();
            xclone.SetAttribute("version", TimetableVersion.JTG3_2.ToNumberString());

            // UPGRADE 008 -> CURRENT
            if (origVersion == TimetableVersion.JTG2_x)
            {
                var shv = xclone.GetAttribute<bool>("shV");
                xclone.SetAttribute("shV", shv ? "1" : "0");

                // km -> kml/kmr
                var sElm = xclone.Children.Single(c => c.XName == "stations");
                foreach (var sta in sElm.Children.Where(x => x.XName == "sta"))
                {
                    var oldPosition = sta.GetAttribute("km", "");
                    sta.SetAttribute("kml", oldPosition!);
                    sta.SetAttribute("kmr", oldPosition!);
                    sta.RemoveAttribute("km");
                }

                // Allocate train ids
                var tElm = xclone.Children.Single(c => c.XName == "trains");
                var tElms = tElm.Children.Where(t => t.XName == "ti" || t.XName == "ta").ToArray();
                var nextId = tElms.DefaultIfEmpty().Max(t => t.GetAttribute("id", -1));
                foreach (var orig in tElms)
                {
                    if (orig.GetAttribute("id", -1) == -1)
                        orig.SetAttribute("id", (++nextId).ToString());
                }
            }
            
            // UPGRADE 009 -> CURRENT is empty
            // UPGRADE 010 -> CURRENT is empty

            // UPGRADE GENERAL
            var ttclone = new Timetable(xclone);
            ColorTimetableConverter.ConvertAll(ttclone);

            return new XMLExport().Export(ttclone, stream, pluginInterface);
        }
    }
}

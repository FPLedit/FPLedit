using FPLedit.Shared;
using FPLedit.Shared.Filetypes;
using System;
using System.IO;
using System.Linq;

namespace FPLedit.BackwardCompat
{
    internal sealed class NetworkUpgradeExport : IExport
    {
        public string Filter => T._("Fahrplan Dateien (*.fpl)|*.fpl");

        public bool Export(Timetable tt, Stream stream, IReducedPluginInterface pluginInterface, string[] flags = null)
        {
            if (tt.Version.GetVersionCompat().Type != TimetableType.Network)
                throw new Exception(T._("Nur Ntzwerk-Fahrplandateien können mit {0} aktualisiert werden!", nameof(NetworkUpgradeExport)));
            if (tt.Version.Compare(TimetableVersion.Extended_FPL2) >= 0)
                throw new Exception(T._("Nur Fahrpläne mit einer älteren Dateiversion können aktualisiert werden."));
            if (tt.Version.CompareTo(TimetableVersion.Extended_FPL) < 0)
                throw new Exception(T._("Dateiversion ist zu alt, um aktualisiert zu werden!"));
            
            var origVersion = tt.Version;

            var xclone = tt.XMLEntity.XClone();
            xclone.SetAttribute("version", TimetableVersion.Extended_FPL2.ToNumberString());

            // UPGRADE 100 --> 101 (CURRENT)
            if (origVersion.CompareTo(TimetableVersion.Extended_FPL2) < 0)
            {
                // Update some properties to time entry format.
                var properties = new[] { "dTt", "odBT", "hlI", "mpP", "sLine" };
                foreach (var prop in properties)
                {
                    if (xclone.Attributes.TryGetValue(prop, out var v) && int.TryParse(v, out var vi))
                    {
                        var te = new TimeEntry(0, vi);
                        xclone.SetAttribute(prop, te.ToString());
                    }
                }
            }

            // UPGRADE GENERAL
            var ttclone = new Timetable(xclone);
            return new XMLExport().Export(ttclone, stream, pluginInterface);
        }
    }
}

using System.IO;
using FPLedit.Shared;

namespace FPLedit.BackwardCompat
{
    public abstract class BaseUpgradeExport : IExport
    {
        public abstract bool Export(Timetable tt, Stream stream, IReducedPluginInterface pluginInterface, string[]? flags = null);

        public string Filter => T._("Fahrplan Dateien (*.fpl)|*.fpl");

        protected void UpgradeTimePrecision(XMLEntity xclone)
        {
            // Update some properties to time entry format.
            var properties = new[] { "dTt", "odBT", "hlI", "mpP", "sLine", "tMin", "tMax" };
            foreach (var prop in properties)
            {
                if (xclone.Attributes.TryGetValue(prop, out var v) && int.TryParse(v, out var vi))
                {
                    var te = new TimeEntry(0, vi);
                    xclone.SetAttribute(prop, te.ToString());
                }
            }
        }
    }
}
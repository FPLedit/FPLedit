using System.IO;
using FPLedit.Shared;
using FPLedit.Shared.Filetypes;

namespace FPLedit.NonDefaultFiletypes
{
    public class StationsOnlyExport : IExport
    {
        public bool Export(Timetable tt, Stream stream, IReducedPluginInterface pluginInterface, string[] flags = null)
        {
            var clone = tt.XMLEntity.XClone(); // Create dissociated copy of this timetable-

            clone.Children.RemoveAll(x => x.XName != "stations");
            
            return new XMLExport().ExportGenericNode(clone, stream, pluginInterface, flags);
        }

        public string Filter => T._("Fahrplan Dateien nur mit Stationen (Streckenexport) (*.fpl)|*.fpl");
    }
}
using System.IO;
using System.Linq;
using FPLedit.Shared;

namespace FPLedit.BackwardCompat;

public abstract class BaseUpgradeExport : IExport
{
    public abstract bool Export(Timetable tt, Stream stream, IReducedPluginInterface pluginInterface, string[]? flags = null);

    public string Filter => T._("Fahrplan Dateien (*.fpl)|*.fpl");

    protected void UpgradeTimePrecision(XMLEntity xclone, bool updateTrainLinks)
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

        if (!updateTrainLinks) 
            return;
        // Update tlo/tld
        var tlProperties = new[] { "tlo", "tld" };
        var trainsElem = xclone.Children.SingleOrDefault(x => x.XName == "trains");
        if (trainsElem == null) 
            return;
            
        foreach (var t in trainsElem.Children)
        {
            var tlElem = t.Children.SingleOrDefault(x => x.XName == "tl");
            if (tlElem == null) 
                continue;
                
            foreach (var prop in tlProperties)
            {
                if (!tlElem.Attributes.TryGetValue(prop, out var v) || !int.TryParse(v, out var vi)) 
                    continue;
                    
                var te = new TimeEntry(0, vi);
                tlElem.SetAttribute(prop, te.ToString());
            }
        }
    }
}
using System.IO;
using System.Xml.Linq;

namespace FPLedit.Shared.Filetypes;

public sealed class XMLImport : IImport
{
    public string Filter => T._("Fahrplan Dateien (*.fpl)|*.fpl");

    public ITimetable? Import(Stream stream, IReducedPluginInterface pluginInterface, ILog? replaceLog = null)
    {
        var xElement = XElement.Load(stream);

        var xmlEntity = new XMLEntity(xElement);
        ITimetable tt = new XmlOnlyTimetable(xmlEntity);

        if (tt.Version.GetVersionCompat().Compatibility == TtVersionCompatType.ReadWrite)
        {
            tt = new Timetable(xmlEntity);
            var actions = pluginInterface.GetRegistered<ITimetableInitAction>();
            foreach (var action in actions)
            {
                var message = action.Init((Timetable)tt, pluginInterface);
                if (message != null)
                    pluginInterface.Logger.Warning(message);
            }
        }

        return tt;
    }
}
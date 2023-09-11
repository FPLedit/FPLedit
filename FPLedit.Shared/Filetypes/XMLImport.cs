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
        var tt = new Timetable(xmlEntity);

        if (tt is Timetable)
        {
            var actions = pluginInterface.GetRegistered<ITimetableInitAction>();
            foreach (var action in actions)
            {
                var message = action.Init(tt, pluginInterface);
                if (message != null)
                    pluginInterface.Logger.Warning(message);
            }
        }

        return tt;
    }
}
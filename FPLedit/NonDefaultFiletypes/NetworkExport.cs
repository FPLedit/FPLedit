using FPLedit.Shared;
using FPLedit.Shared.Filetypes;
using System.Collections.Generic;
using System.IO;

namespace FPLedit.NonDefaultFiletypes;

internal sealed class NetworkExport : BaseConverterFileType, IExport
{
    public string Filter => T._("Fahrplan Dateien (*.fpl)|*.fpl");

    public bool Export(ITimetable tt, Stream stream, IReducedPluginInterface pluginInterface, string[]? flags = null)
    {
        if (tt.Type == TimetableType.Network)
            throw new TimetableTypeNotSupportedException(TimetableType.Network, "convert to network");

        if (tt.Version.CompareTo(TimetableVersion.JTG3_3) <= 0)
        {
            pluginInterface.Logger.Error(T._("Bitte zuerst die lineare Strecke auf die neueste Version aktualisieren!"));
            return false;
        }

        var clone = tt.Clone();

        var trainPaths = new Dictionary<ITrain, TrainPathData>();
        foreach (var orig in clone.Trains)
            trainPaths[orig] = new TrainPathData(clone, orig);

        var rt = Timetable.LINEAR_ROUTE_ID.ToString();
        var id = 0;
        var y = 0;
        foreach (var sta in clone.Stations)
        {
            ConvertStationLinToNet(sta);

            sta.SetAttribute("fpl-rt", rt); // Normally this would need a cache invalidation, but here it does not.
            sta.SetAttribute("fpl-pos", (y += 40).ToString() + ";0");
            sta.SetAttribute("fpl-id", id++.ToString());
        }

        var actions = pluginInterface.GetRegistered<ITimetableTypeChangeAction>();
        foreach (var action in actions)
            action.ToNetwork(clone);

        clone.SetVersion(TimetableVersion.Extended_FPL2);

        foreach (var train in clone.Trains)
        {
            var data = trainPaths[train];

            if (!(train is IWritableTrain wt))
                continue;

            wt.Children.Clear();
            wt.AddAllArrDeps(data.GetRawPath());
            wt.XMLEntity.XName = "tr";

            foreach (var sta in data.PathEntries)
            {
                if (sta.ArrDep != null)
                    wt.GetArrDep(sta.Station).ApplyCopy(sta.ArrDep);
            }
        }

        return new XMLExport().Export(clone, stream, pluginInterface);
    }
}
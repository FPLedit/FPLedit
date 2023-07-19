using FPLedit.Shared;
using FPLedit.Shared.Filetypes;
using System;
using System.IO;
using System.Linq;

namespace FPLedit.BackwardCompat;

internal sealed class NetworkUpgradeExport : BaseUpgradeExport
{
    public override bool Export(Timetable tt, Stream stream, IReducedPluginInterface pluginInterface, string[]? flags = null)
    {
        if (tt.Version.GetVersionCompat().Type != TimetableType.Network)
            throw new Exception(T._("Nur Netzwerk-Fahrplandateien können mit {0} aktualisiert werden!", nameof(NetworkUpgradeExport)));
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
            UpgradeTimePrecision(xclone, false); // We had no train link support in network timetable version=100.

            // Bug in FPLedit 2.1 muss nachträglich klar gemacht werden.
            // Durch Nutzerinteraction konnten "ambiguous routes" entstehen.
            // Eine Korrektur ist nicht möglich.
            // Das Format Extended_FPL2 wurtde mit Version 2.3.0 eingeführt, der Fix hier mit v2.2.0 ausgerollt.
            if (tt.Type == TimetableType.Network && tt.Version == TimetableVersion.Extended_FPL && tt.HasRouteCycles)
            {
                // All stations that are junction points.
                var maybeAffectedRoutes = tt.GetCyclicRoutes();
                var junctions = tt.Stations.Where(s => s.IsJunction && s.Routes.Intersect(maybeAffectedRoutes).Any()).ToArray();
                var hasAmbiguousRoutes = tt.CheckAmbiguousRoutesInternal(junctions);

                if (hasAmbiguousRoutes)
                    pluginInterface.Logger.Warning(T._("Die Datei enthält zusammengefallene Strecken, das heißt zwei Stationen sind auf mehr als einer Route ohne Zwischenstation verbunden. FPLedit kann sich danach komisch verhalten und Züge zufällig über die eine oder andere Strecke leiten. Eine Korrektur ist leider nicht möglich."));
            }
        }

        // UPGRADE GENERAL
        var ttclone = new Timetable(xclone);
        return new XMLExport().Export(ttclone, stream, pluginInterface);
    }
}
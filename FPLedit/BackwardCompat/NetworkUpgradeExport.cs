﻿using FPLedit.Shared;
using System;
using System.IO;
using System.Linq;

namespace FPLedit.BackwardCompat;

internal sealed class NetworkUpgradeExport : BaseUpgradeExport
{
    protected override Timetable PerformUpgrade(XMLEntity xclone, TimetableVersion origVersion, Stream stream, IReducedPluginInterface pluginInterface, string[]? flags = null)
    {
        if (origVersion.GetVersionCompat().Type != TimetableType.Network)
            throw new Exception(T._("Nur Netzwerk-Fahrplandateien können mit {0} aktualisiert werden!", nameof(NetworkUpgradeExport)));
        if (origVersion.Compare(TimetableVersion.Extended_FPL2) >= 0)
            throw new Exception(T._("Nur Fahrpläne mit einer älteren Dateiversion können aktualisiert werden."));
        if (origVersion.CompareTo(TimetableVersion.Extended_FPL) < 0)
            throw new Exception(T._("Dateiversion ist zu alt, um aktualisiert zu werden!"));

        xclone.SetAttribute("version", TimetableVersion.Extended_FPL2.ToNumberString());

        // UPGRADE 100 --> 101 (CURRENT)
        if (origVersion.CompareTo(TimetableVersion.Extended_FPL2) < 0)
        {
            UpgradeTimePrecision(xclone, false); // We had no train link support in network timetable version=100.
        }

        // Load new XML as Timetable instance.
        var ttclone = new Timetable(xclone);

        // POST UPGRADE 100 --> 101 (CURRENT): Check for bugs!
        // Do this only after initializing ttclone, as we need a fully initialized R/W Timetable instance for that.
        if (origVersion.CompareTo(TimetableVersion.Extended_FPL2) < 0)
        {
            // Bug in FPLedit 2.1 muss nachträglich klar gemacht werden.
            // Durch Nutzerinteraction konnten "ambiguous routes" entstehen.
            // Eine Korrektur ist nicht möglich.
            // Das Format Extended_FPL2 wurtde mit Version 2.3.0 eingeführt, der Fix hier mit v2.2.0 ausgerollt.
            if (origVersion == TimetableVersion.Extended_FPL && ttclone is { Type: TimetableType.Network, HasRouteCycles: true })
            {
                // All stations that are junction points.
                var maybeAffectedRoutes = ttclone.GetCyclicRoutes();
                var junctions = ttclone.Stations.Where(s => s.IsJunction && s.Routes.Intersect(maybeAffectedRoutes).Any()).ToArray();
                var hasAmbiguousRoutes = ttclone.CheckAmbiguousRoutesInternal(junctions);

                if (hasAmbiguousRoutes)
                    pluginInterface.Logger.Warning(T._("Die Datei enthält zusammengefallene Strecken, das heißt zwei Stationen sind auf mehr als einer Route ohne Zwischenstation verbunden. FPLedit kann sich danach komisch verhalten und Züge zufällig über die eine oder andere Strecke leiten. Eine Korrektur ist leider nicht möglich."));
            }
        }

        return ttclone;
    }
}
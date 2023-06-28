using System;
using System.Collections.Generic;
using FPLedit.GTFS.GTFSLib;
using FPLedit.Shared;
using GtfsRoute = FPLedit.GTFS.GTFSLib.Route;

namespace FPLedit.GTFS;

public class GtfsExport
{
    public static void X(Timetable tt)
    {
        var line = tt.GetRoute(Timetable.LINEAR_ROUTE_ID);

        var file = new GtfsFile();

        file.Agency = new Agency { AgencyName = "TestAgency", AgencyLang = "de", AgencyUrl = "https://example.com", AgencyTimezone = "Europa/Berlin" };
        file.Route = new GtfsRoute { RouteId = "TestStrecke", RouteShortName = "TestStrecke", RouteType = RouteType.Rail };

        var gtfsStops = new Dictionary<IStation, Stop>();
        foreach (var s in line.Stations)
            gtfsStops.Add(s, Stop.FromStation(s, 0, 0));
        file.Stops.AddRange(gtfsStops.Values);

        foreach (var train in tt.Trains)
        {
            var c = Calendar.FromTrain(train, false, new DateOnly(2023, 01, 01), new DateOnly(2023, 01, 01));
            file.Calendars.Add(c);

            var trip = Trip.FromTrain(file.Route, c, train);
            file.Trips.Add(trip);

            file.StopTimes.AddRange(StopTime.FromTrain(trip, train, gtfsStops));
        }
    }
}
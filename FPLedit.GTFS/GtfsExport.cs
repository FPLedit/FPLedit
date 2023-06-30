using System.Collections.Generic;
using FPLedit.GTFS.GTFSLib;
using FPLedit.GTFS.Model;
using FPLedit.Shared;
using GtfsRoute = FPLedit.GTFS.GTFSLib.Route;

namespace FPLedit.GTFS;

public class GtfsExport
{
    public static void X(Timetable tt)
    {
        if (tt.Type != TimetableType.Linear)
            throw new TimetableTypeNotSupportedException(TimetableType.Network, "GTFS Export");

        var attTt = GtfsAttrs.GetAttrs(tt);
        var line = tt.GetRoute(Timetable.LINEAR_ROUTE_ID);

        var file = new GtfsFile();

        file.Agency = new Agency { AgencyName = attTt.AgencyName, AgencyLang = attTt.AgencyLang, AgencyUrl = attTt.AgencyUrl, AgencyTimezone = attTt.AgencyTimezone };
        file.Route = new GtfsRoute { RouteId = GtfsField.ToId(attTt.RouteName), RouteShortName = attTt.RouteName, RouteType = RouteType.Rail };

        var gtfsStops = new Dictionary<IStation, Stop>();
        foreach (var s in line.Stations)
            gtfsStops.Add(s, Stop.FromStation(s, 0, 0));
        file.Stops.AddRange(gtfsStops.Values);

        foreach (var train in tt.Trains)
        {
            var attTrain = new GtfsTrainAttrs(train);
            var daysOverride = GtfsDays.Parse(attTrain.DaysOverride) ?? GtfsDays.Empty;
            var c = Calendar.FromTrain(train, !daysOverride.IsRange, daysOverride.StartDate, daysOverride.EndDate);
            if (!daysOverride.IsRange)
            {
                foreach (var dt in daysOverride.IrregularDays)
                    file.CalendarDates.Add(new CalendarDate() { Service = c, Date = dt, ExceptionType = CalendarDateType.Added });
            }
            file.Calendars.Add(c);

            var trip = Trip.FromTrain(file.Route, c, train);
            if (attTrain.WheelchairAccessible != AccessibilityState.NotDefined)
                trip.WheelchairAccessible = attTrain.WheelchairAccessible;
            if (attTrain.BikesAllowed != AccessibilityState.NotDefined)
                trip.BikesAllowed = attTrain.BikesAllowed;
            file.Trips.Add(trip);

            file.StopTimes.AddRange(StopTime.FromTrain(trip, train, gtfsStops));
        }

        var files = file.Write();
    }
}
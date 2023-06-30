using System.Collections.Generic;
using System.IO;
using System.Linq;
using FPLedit.GTFS.GeoProvider;
using FPLedit.GTFS.GTFSLib;
using FPLedit.GTFS.Model;
using FPLedit.Shared;
using GtfsRoute = FPLedit.GTFS.GTFSLib.Route;

namespace FPLedit.GTFS;

public class GtfsExport
{
    public static void X(Timetable tt, ILog log, string filename)
    {
        if (tt.Type != TimetableType.Linear)
            throw new TimetableTypeNotSupportedException(TimetableType.Network, "GTFS Export");

        var attTt = GtfsAttrs.GetAttrs(tt);
        if (attTt == null)
        {
            log.Error(T._("GTFS-Export: Keine Agency/Streckendaten vorhanden!"));
            return;
        }
        var line = tt.GetRoute(Timetable.LINEAR_ROUTE_ID);

        if (string.IsNullOrEmpty(attTt.AgencyName) || string.IsNullOrEmpty(attTt.AgencyLang) || string.IsNullOrEmpty(attTt.AgencyUrl) || string.IsNullOrEmpty(attTt.AgencyTimezone) || string.IsNullOrEmpty(attTt.RouteName))
            log.Warning(T._("GTFS-Export: Unvollständige Agency/Streckendaten!"));

        IGeoProvider? geo = null;
        if (File.Exists(filename + ".kml"))
            geo = new KmlGeoProvider(filename + ".kml");
        else
            geo = new ZeroGeoProvider();

        var file = new GtfsFile();

        file.Agency = new Agency { AgencyName = attTt.AgencyName, AgencyLang = attTt.AgencyLang, AgencyUrl = attTt.AgencyUrl, AgencyTimezone = attTt.AgencyTimezone };
        file.Route = new GtfsRoute { RouteId = GtfsField.ToId(attTt.RouteName), RouteShortName = attTt.RouteName, RouteType = RouteType.Rail };
        var routeGeo = geo.GetGeoLine(attTt.RouteName);

        Shape? lastShape = null;
        if (routeGeo.Any())
        {
            foreach (var point in routeGeo)
            {
                lastShape = new Shape(point.lat, point.lon, lastShape) { ShapeId = GtfsField.ToId(attTt.RouteName) };
                file.Shapes.Add(lastShape);
            }
        }
        
        var gtfsStops = new Dictionary<IStation, Stop>();
        foreach (var s in line.Stations)
        {
            var g = geo.GetGeoPoint(s.SName);
            gtfsStops.Add(s, Stop.FromStation(s, g?.lat ?? 0f, g?.lon ?? 0f));
        }

        file.Stops.AddRange(gtfsStops.Values);

        foreach (var train in tt.Trains)
        {
            var attTrain = new GtfsTrainAttrs(train);
            var daysOverride = GtfsDays.Parse(attTrain.DaysOverride) ?? GtfsDays.Empty;
            var c = Calendar.FromTrain(train, !daysOverride.IsRange, daysOverride.StartDate, daysOverride.EndDate);
            if (!daysOverride.IsRange)
            {
                foreach (var dt in daysOverride.IrregularDays)
                    file.CalendarDates.Add(new CalendarDate { Service = c, Date = dt, ExceptionType = CalendarDateType.Added });
            }
            file.Calendars.Add(c);

            var trip = Trip.FromTrain(file.Route, c, train);
            if (lastShape != null)
                trip.Shape = lastShape;
            if (attTrain.WheelchairAccessible != AccessibilityState.NotDefined)
                trip.WheelchairAccessible = attTrain.WheelchairAccessible;
            if (attTrain.BikesAllowed != AccessibilityState.NotDefined)
                trip.BikesAllowed = attTrain.BikesAllowed;
            file.Trips.Add(trip);

            file.StopTimes.AddRange(StopTime.FromTrain(trip, train, gtfsStops));
        }

        var files = file.GetFiles();
    }
}
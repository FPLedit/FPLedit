using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FPLedit.GTFS.GeoProvider;
using FPLedit.GTFS.GTFSLib;
using FPLedit.GTFS.Model;
using FPLedit.Shared;
using GtfsRoute = FPLedit.GTFS.GTFSLib.Route;
using Route = FPLedit.Shared.Route;

namespace FPLedit.GTFS;

public static class GtfsExport
{
    public static bool Export(Timetable tt, ILog log, string filename, string exportFolder)
    {
        if (tt.Type != TimetableType.Linear)
            throw new TimetableTypeNotSupportedException(TimetableType.Network, "GTFS Export");

        if (!Directory.Exists(exportFolder))
            Directory.CreateDirectory(exportFolder);

        var attTt = GtfsAttrs.GetAttrs(tt);
        if (attTt == null)
        {
            log.Error(T._("GTFS-Export: Keine Agency/Streckendaten vorhanden!"));
            return false;
        }
        var line = tt.GetRoute(Timetable.LINEAR_ROUTE_ID);

        if (!attTt.HasAllRequiredFields)
            log.Warning(T._("GTFS-Export: Unvollständige Agency/Streckendaten!"));

        // Load geo provider, if the kml sidecar file exists.
        IGeoProvider geo;
        if (File.Exists(filename + ".kml"))
        {
            log.Info(T._("Benutze KML-Datei {0}", filename + ".kml"));
            geo = new KmlGeoProvider(filename + ".kml");
        }
        else
        {
            log.Warning(T._("Keine KML-Datei {0} gefunden!", filename + ".kml"));
            geo = new ZeroGeoProvider();
        }

        var file = GetGtfsFeedContents(tt, attTt, geo, line);

        var missedQueries = geo.GetMissedQueries().ToArray();
        if (missedQueries.Any())
            log.Warning(T._("Nicht gefundene Geo-Angaben: {0}", string.Join(", ", missedQueries)));

        // Write out the generated feed and warn for existing files.
        var existingFiles = Directory.GetFiles(exportFolder).Select(Path.GetFileName).ToArray();
        var files = file.GetFiles();
        var filesToDelete = existingFiles.Except(files.Keys).ToArray();
        if (filesToDelete.Any())
            log.Warning(T._("Zusätzlich vorhandene Dateien im Zielordner: {0}", string.Join(", ", filesToDelete)));

        foreach (var f in files)
            File.WriteAllText(Path.Combine(exportFolder, f.Key), f.Value);

        return true;
    }

    private static GtfsFile GetGtfsFeedContents(Timetable tt, GtfsAttrs attTt, IGeoProvider geo, Route line)
    {
        var file = new GtfsFile();
        var routeTypeValid = Enum.TryParse(attTt.RouteType, out RouteType routeType);

        file.Agency = new Agency { AgencyName = attTt.AgencyName, AgencyLang = attTt.AgencyLang, AgencyUrl = attTt.AgencyUrl, AgencyTimezone = attTt.AgencyTimezone };
        file.Route = new GtfsRoute { RouteId = GtfsField.ToId(attTt.RouteName), RouteShortName = attTt.RouteName, RouteType = routeTypeValid ? routeType : RouteType.Rail };
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
            var daysOverrideString = string.IsNullOrEmpty(attTrain.DaysOverride) ? attTt.DaysOverride : attTrain.DaysOverride;
            var daysOverride = GtfsDays.Parse(daysOverrideString) ?? GtfsDays.Empty;

            var calendarsToUse = new List<ICalendar>();
            if (daysOverride.IsRange)
            {
                var c = Calendar.FromTrain(train, daysOverride.StartDate, daysOverride.EndDate);
                file.Calendars.Add(c);
                calendarsToUse.Add(c);
            }
            else
            {
                foreach (var dt in daysOverride.IrregularDays)
                {
                    var c = new CalendarDate { ServiceId = train.TName + "__" + dt.ToString("yyyyMMdd"), Date = dt, ExceptionType = CalendarDateType.Added };
                    file.CalendarDates.Add(c);
                    calendarsToUse.Add(c);
                }
            }

            foreach (var c in calendarsToUse)
            {
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
        }

        return file;
    }
}
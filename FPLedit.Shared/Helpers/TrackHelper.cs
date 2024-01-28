using System;

namespace FPLedit.Shared.Helpers;

[Templating.TemplateSafe]
public sealed class TrackHelper
{
    public string? GetTrack(TrainPathData path, Station sta, TrainDirection? dir, ArrDep timetableEntry, TrackQuery track)
    {
        if (!path.ContainsStation(sta))
            return null;

        Station[]? pathDisc = null;
        if (dir == null)
            pathDisc = path.GetSurroundingStations(sta, 1);

        if (track == TrackQuery.Departure)
        {
            var exitRoute = path.GetExitRoute(sta);

            var exitDir = dir;
            if (dir == null && exitRoute != Timetable.UNASSIGNED_ROUTE_ID)
            {
                if (pathDisc!.Length < 2 || pathDisc[^2] != sta) throw new Exception("Unexpected path state");
                var p1 = pathDisc[^1].Positions.GetPosition(exitRoute);
                var p2 = sta.Positions.GetPosition(exitRoute);
                exitDir = p2 > p1 ? TrainDirection.ta : TrainDirection.ti;
            }

            return Fallback(timetableEntry.DepartureTrack,
                Fallback(timetableEntry.ArrivalTrack,
                    Fallback(exitDir == TrainDirection.ti
                        ? sta.DefaultTrackRight.GetValue(exitRoute)
                        : sta.DefaultTrackLeft.GetValue(exitRoute), "")));
        }

        var entryRoute = path.GetEntryRoute(sta);

        var entryDir = dir;
        if (entryRoute != Timetable.UNASSIGNED_ROUTE_ID)
        {
            if (pathDisc!.Length < 2 || pathDisc[1] != sta) throw new Exception("Unexpected path state");
            var p1 = pathDisc[0].Positions.GetPosition(entryRoute);
            var p2 = sta.Positions.GetPosition(entryRoute);
            entryDir = p1 > p2 ? TrainDirection.ta : TrainDirection.ti;
        }

        return Fallback(timetableEntry.ArrivalTrack,
            Fallback(timetableEntry.DepartureTrack,
                Fallback(entryDir == TrainDirection.ti
                    ? sta.DefaultTrackLeft.GetValue(entryRoute)
                    : sta.DefaultTrackRight.GetValue(entryRoute), "")));
    }

    private string? Fallback(string? test, string? fallback) => string.IsNullOrEmpty(test) ? fallback : test;
}

[Templating.TemplateSafe]
public enum TrackQuery
{
    Arrival,
    Departure
}
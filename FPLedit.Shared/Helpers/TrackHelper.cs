namespace FPLedit.Shared.Helpers;

[Templating.TemplateSafe]
public sealed class TrackHelper
{
    public string? GetTrack(TrainPathData path, Station sta, TrainDirection dir, ArrDep timetableEntry, TrackQuery track)
    {
        if (!path.ContainsStation(sta))
            return null;

        var exitRoute = path.GetExitRoute(sta);

        if (track == TrackQuery.Departure)
        {
            return Fallback(timetableEntry.DepartureTrack,
                Fallback(timetableEntry.ArrivalTrack,
                    Fallback(dir == TrainDirection.ti 
                        ? sta.DefaultTrackRight.GetValue(exitRoute) 
                        : sta.DefaultTrackLeft.GetValue(exitRoute), "")));
        }

        return Fallback(timetableEntry.ArrivalTrack,
            Fallback(timetableEntry.DepartureTrack,
                Fallback(dir == TrainDirection.ti 
                    ? sta.DefaultTrackRight.GetValue(exitRoute) 
                    : sta.DefaultTrackLeft.GetValue(exitRoute), "")));
    }

    private string? Fallback(string? test, string? fallback) => string.IsNullOrEmpty(test) ? fallback : test;
}

[Templating.TemplateSafe]
public enum TrackQuery
{
    Arrival,
    Departure
}
using System;

namespace FPLedit.Shared.Filetypes
{
    public abstract class BaseConverterFileType
    {
        protected void ConvertStationNetToLin(Station sta, int route, TimetableVersion targetVersion)
        {
            float km = sta.Positions.GetPosition(route) ?? throw new Exception($"The station {sta.SName} has no position entry on route {route}!");
            sta.Positions.SetPosition(Timetable.LINEAR_ROUTE_ID, km);
            sta.Positions.Write(TimetableType.Linear, targetVersion);

            ConvertAttrNetToLin(sta.Wellenlinien, route);
            ConvertAttrNetToLin(sta.Vmax, route);
            ConvertAttrNetToLin(sta.LineTracksRight, route);
            ConvertAttrNetToLin(sta.DefaultTrackLeft, route);
            ConvertAttrNetToLin(sta.DefaultTrackRight, route);
        }

        protected void ConvertStationLinToNet(Station sta)
        {
            sta.Positions.Write(TimetableType.Network);

            ConvertAttrLinToNet(sta.Vmax);
            ConvertAttrLinToNet(sta.Wellenlinien);
            ConvertAttrLinToNet(sta.LineTracksRight);
            ConvertAttrLinToNet(sta.DefaultTrackLeft);
            ConvertAttrLinToNet(sta.DefaultTrackRight);
        }

        protected void ConvertAttrNetToLin<T>(RouteValueCollection<T> rvc, int route)
        {
            T val = rvc.GetValue(route);
            rvc.SetValue(Timetable.LINEAR_ROUTE_ID, val);
            rvc.Write(TimetableType.Linear);
        }

        protected void ConvertAttrLinToNet<T>(RouteValueCollection<T> rvc) => rvc.Write(TimetableType.Network);
    }
}

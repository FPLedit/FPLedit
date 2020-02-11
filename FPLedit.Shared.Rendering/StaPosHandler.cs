using Eto.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FPLedit.Shared.Rendering
{
    /// <summary>
    /// Manages the display position for station rendering.
    /// </summary>
    public sealed class StaPosHandler
    {
        private Point GetPoint(Station sta)
        {
            var val = sta.GetAttribute("fpl-pos", "0;0");
            var p = val.Split(';');
            if (p.Length != 2)
                throw new FormatException($"Falsche Positionsangabe {val}!");
            return new Point(int.Parse(p[0]), int.Parse(p[1]));
        }

        public Dictionary<Station, Point> LoadNetworkPoints(Timetable tt)
        {
            var ret = new Dictionary<Station, Point>();

            foreach (var sta in tt.Stations)
                ret.Add(sta, GetPoint(sta));
            return ret;
        }

        public Dictionary<Station, Point> GenerateLinearPoints(Timetable tt, int width)
        {
            var ret = new Dictionary<Station, Point>();
            var d = (width - 80) / Math.Max(tt.Stations.Count - 1, 1);
            int x = 0;
            foreach (var sta in tt.Stations)
            {
                ret.Add(sta, new Point(x, 0));
                x += d;
            }
            return ret;
        }

        public void WriteStapos(Timetable tt, Dictionary<Station, Point> stapos)
        {
            if (tt.Type == TimetableType.Linear)
                return;

            foreach (var s in stapos)
            {
                if (!tt.Stations.Contains(s.Key))
                    continue;
                var val = s.Value.X.ToString() + ";" + s.Value.Y.ToString();
                s.Key.SetAttribute("fpl-pos", val);
            }
        }

        public void SetMiddlePos(int route, Station m, Timetable tt)
        {
            var km = m.Positions.GetPosition(route).Value;
            var s1 = GetStationBefore(route, km, tt);
            var s2 = GetStationAfter(route, km, tt);

            Point pm;
            if (s1 == null && s2 == null)
                pm = new Point(0, 0);
            else if (s1 == null)
            {
                var p2 = GetPoint(s2);
                pm = new Point(p2.X - 80, p2.Y);
            }
            else if (s2 == null)
            {
                var p1 = GetPoint(s1);
                pm = new Point(p1.X + 80, p1.Y);
            }
            else
            {
                var p1 = GetPoint(s1);
                var p2 = GetPoint(s2);
                var x = (p1.X - p2.X) / 2;
                var y = (p1.Y - p2.Y) / 2;
                pm = new Point(p1.X - x, p1.Y - y);
            }

            var val = pm.X.ToString() + ";" + pm.Y.ToString();
            m.SetAttribute("fpl-pos", val);
        }

        private Station GetStationBefore(int route, float km, Timetable tt)
            => tt.Stations.LastOrDefault(s =>
                s.Routes.Contains(route) && s.Positions.GetPosition(route) < km);

        Station GetStationAfter(int route, float km, Timetable tt)
            => tt.Stations.FirstOrDefault(s =>
                s.Routes.Contains(route) && s.Positions.GetPosition(route) > km);
    }
}

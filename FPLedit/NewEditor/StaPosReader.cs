using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace FPLedit
{
    internal class StaPosReader
    {
        public Dictionary<Station, Point> LoadNetworkPoints(Timetable tt)
        {
            var ret = new Dictionary<Station, Point>();

            foreach (var sta in tt.Stations)
            {
                var val = sta.GetAttribute("fpl-pos", "0;0");
                var p = val.Split(';');
                if (p.Length != 2)
                    throw new FormatException("Falsche Positionsangabe!");
                ret.Add(sta, new Point(int.Parse(p[0]), int.Parse(p[1])));
            }
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
            foreach (var s in stapos)
            {
                if (!tt.Stations.Contains(s.Key))
                    continue;
                var val = s.Value.X.ToString() + ";" + s.Value.Y.ToString();
                s.Key.SetAttribute("fpl-pos", val);
            }
        }
    }
}

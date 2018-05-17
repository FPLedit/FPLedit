using FPLedit.BildfahrplanExport.Model;
using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace FPLedit.BildfahrplanExport.Render
{
    internal class TrainRenderer
    {
        private List<Station> stations; // Selected route, ordered
        private Timetable tt;
        private Margins margin = new Margins(10, 20, 20, 20);
        private TimetableStyle attrs;
        private TimeSpan startTime;
        private Dictionary<Station, float> stationOffsets;

        private DashStyleHelper ds = new DashStyleHelper();

        public TrainRenderer(List<Station> stations, Timetable tt, Margins margin, TimeSpan startTime, Dictionary<Station, float> stationOffsets)
        {
            this.stations = stations;
            this.tt = tt;
            this.margin = margin;
            this.startTime = startTime;
            this.stationOffsets = stationOffsets;
            attrs = new TimetableStyle(tt);
        }

        public void Render(Graphics g, Train train)
        {
            var style = new TrainStyle(train);
            if (!style.Show)
                return;

            var ardps = train.GetArrDeps();

            int tWidth = style.TrainWidth ?? attrs.TrainWidth;
            List<PointF> points = new List<PointF>();
            foreach (var sta in stations)
            {
                if (!ardps.ContainsKey(sta))
                    continue;
                var ardp = ardps[sta];

                var tmpPoints = new List<PointF>(4);
                float x = margin.Left + stationOffsets[sta];

                foreach (var time in new[] { ardp.Departure, ardp.Arrival })
                {
                    if (time == new TimeSpan())
                        continue;
                    TimeSpan timeOffset = time - startTime;
                    float y = margin.Top + timeOffset.GetMinutes() * attrs.HeightPerHour / 60f;
                    tmpPoints.Add(new PointF(x, y));
                }

                if (!GetTrainDirection(train))
                    tmpPoints.Reverse();

                if (tmpPoints.Count == 2 && tmpPoints[0] != tmpPoints[1] && attrs.StationLines)
                    tmpPoints.InsertRange(1, tmpPoints);

                points.AddRange(tmpPoints);
            }

            var pen = new Pen(style.TrainColor ?? attrs.TrainColor, tWidth);
            pen.DashPattern = ds.ParseDashstyle(style.LineStyle);
            var brush = new SolidBrush(style.TrainColor ?? attrs.TrainColor);

            for (int i = 0; i < points.Count; i += 2)
            {
                if (points.Count <= i + 1)
                    continue;

                g.DrawLine(pen, points[i], points[i + 1]);

                if (points[i].X == points[i + 1].X)
                    continue;
                var size = g.MeasureString(train.TName, attrs.TrainFont);
                float[] ys = new[] { points[i].Y, points[i + 1].Y };
                float[] xs = new[] { points[i].X, points[i + 1].X };
                float y = ys.Min() + (ys.Max() - ys.Min()) / 2 - (size.Height / 2);
                float x = xs.Min() + (xs.Max() - xs.Min()) / 2;

                float angle = (float)(Math.Atan2(xs.Max() - xs.Min(), ys.Max() - ys.Min()) * (180d / Math.PI));
                angle = GetTrainDirection(train) ? 90 - angle : angle - 90;
                var container = g.BeginContainer();
                g.TranslateTransform(x, y);
                g.RotateTransform(-angle);
                g.DrawString(train.TName, attrs.TrainFont, brush, -(size.Width / 2), -(size.Height / 2));
                g.EndContainer(container);
            }
        }

        #region Direction helpers
        private bool GetTrainDirection(Train train)
        {
            if (tt.Type == TimetableType.Linear)
                return train.Direction == TrainDirection.ta;

            return GetTrains(TrainDirection.ta).Contains(train);
        }

        private Station[] GetStationsInDir(TrainDirection dir)
        {
            if (tt.Type == TimetableType.Linear)
                return tt.GetStationsOrderedByDirection(dir).ToArray();

            var route = stations.ToList(); // Kopie erzeugen
            if (dir == TrainDirection.ta)
                route.Reverse();

            return route.ToArray();
        }

        private Train[] GetTrains(TrainDirection dir)
        {
            var stasAfter = GetStationsInDir(dir);
            return tt.Trains.Where(t =>
            {
                var p = t.GetPath();
                var ardeps = t.GetArrDeps();

                var intersect = stasAfter.Intersect(p)
                    .Where(s => ardeps[s].HasMinOneTimeSet);

                if (intersect.Count() == 0)
                    return false;

                var time1 = ardeps[intersect.First()].FirstSetTime;
                var time2 = ardeps[intersect.Last()].FirstSetTime;

                return time1 < time2;
            }).ToArray();
        }
        #endregion
    }
}

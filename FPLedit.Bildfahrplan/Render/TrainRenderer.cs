using FPLedit.Bildfahrplan.Model;
using FPLedit.Shared;
using FPLedit.Shared.Rendering;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using FPLedit.Bildfahrplan.Helpers;

namespace FPLedit.Bildfahrplan.Render
{
    internal sealed class TrainRenderer
    {
        private readonly IEnumerable<Station> stations; // Selected route, ordered
        private readonly Timetable tt;
        private readonly Margins margin;
        private readonly TimetableStyle attrs;
        private readonly TimeEntry startTime;
        private readonly Dictionary<Station, StationRenderProps> stationOffsets;
        private readonly Days renderDays;
        private ITrain[] trainCache;

        private readonly DashStyleHelper ds = new();

        private readonly float clipTop;
        private readonly float clipBottom;

        public TrainRenderer(IEnumerable<Station> stations, Timetable tt, Margins margin, TimeEntry startTime, Dictionary<Station, StationRenderProps> stationOffsets, Days renderDays, float clipTop, float clipBottom)
        {
            this.stations = stations;
            this.tt = tt;
            this.margin = margin;
            this.startTime = startTime;
            this.stationOffsets = stationOffsets;
            this.renderDays = renderDays;
            this.clipTop = clipTop;
            this.clipBottom = clipBottom;
            attrs = new TimetableStyle(tt);
        }

        public void Render(Graphics g, ITrain train, bool exportColor)
        {
            var style = new TrainStyle(train, attrs);
            if (!style.CalcedShow)
                return;

            var ardps = train.GetArrDepsUnsorted();
            var dir = GetTrainDirection(train);

            using var pen = new Pen(style.CalcedColor.ToSD(exportColor), style.CalcedWidth)
            {
                DashPattern = ds.ParseDashstyle(style.CalcedLineStyle)
            };
            using var brush = new SolidBrush(style.CalcedColor.ToSD(exportColor));
            
            List<PointF> points = new List<PointF>();
            bool hadFirstArrival = false, hadLastDeparture = false, isFirst = true;
            var stas = dir ? Enumerable.Reverse(stations) : stations;

            int trainTravelsRouteCount = 0;
            foreach (var sta in stas)
            {
                if (!ardps.ContainsKey(sta))
                    continue;
                var ardp = ardps[sta];
                trainTravelsRouteCount++;

                if (!ardp.HasMinOneTimeSet)
                    continue;

                MaybeAddPoint(points, GetGutterPoint(true, dir, stationOffsets[sta], ardp.Arrival));
                MaybeAddPoint(points, GetInternalPoint(stationOffsets[sta], ardp.Arrival, ardp.ArrivalTrack));

                foreach (var shunt in ardp.ShuntMoves)
                {
                    MaybeAddPoint(points, GetInternalPoint(stationOffsets[sta], shunt.Time, shunt.SourceTrack));
                    MaybeAddPoint(points, GetInternalPoint(stationOffsets[sta], shunt.Time, shunt.TargetTrack));
                }

                MaybeAddPoint(points, GetInternalPoint(stationOffsets[sta], ardp.Departure, ardp.DepartureTrack));
                MaybeAddPoint(points, GetGutterPoint(false, dir, stationOffsets[sta], ardp.Departure));

                hadLastDeparture = ardp.Departure != default;
                if (isFirst)
                    hadFirstArrival = ardp.Arrival != default;
                isFirst = false;
            }

            // Halbe Linien bei Abfahrten / Ankünften ohne Gegenstelle
            if (attrs.DrawNetworkTrains)
            {
                var hly = !dir ? 20 : -20;
                if (hadLastDeparture)
                    points.Add(points.Last() + new Size(50, hly));
                if (hadFirstArrival)
                    points.Insert(0, points.First() - new Size(50, hly));
            }
            else if (trainTravelsRouteCount <= 1)
                return; // This train has only one station on this route and we don't draw network trains.

            if (points.Count == 0)
                return; // This train is not travelling on this route

            // Transition to the next train; filtered by days and station.
            var lastStaOfFirst = GetSortedStations(train)?.LastOrDefault();
            var transition = tt.GetTransition(train, renderDays, lastStaOfFirst);
            if (transition != null && !hadLastDeparture && attrs.StationLines != StationLineStyle.None && transition.Days.IsIntersecting(renderDays))
            {
                var firstStaOfNext = GetSortedStations(transition)?.FirstOrDefault();

                if (lastStaOfFirst == firstStaOfNext)
                {
                    var departure = transition.GetArrDep(firstStaOfNext).Departure;
                    points.Add(new PointF(points.Last().X, GetTimeY(departure)));
                }
            }

            using var p = new GraphicsPath();
            
            for (int i = 0; i < points.Count; i += 1)
            {
                if (points.Count <= i + 1)
                    continue;

                // Skip lines that are totally outside the rendering area.
                if (points[i].Y < clipTop && points[i + 1].Y < clipTop)
                    continue;
                if (points[i].Y > clipBottom && points[i + 1].Y > clipBottom)
                    continue;

                var isStationLine = (int)points[i].X == (int)points[i + 1].X; // explicitely don't use op1/2 for state calculations.
                var (cp1, cp2) = GetClippedPointsForLine(points[i], points[i + 1]);

                if (isStationLine) // Line in one station.
                {
                    var preX = i > 0 ? points[i - 1].X : 0;
                    var postX = i < points.Count - 2 ? points[i + 2].X : 0;
                    var curX = points[i].X; // explicitely don't use op1 for state calculations.
                    var isTransition = isStationLine && (points.Count == i + 2 || Math.Sign(preX - curX) == Math.Sign(postX - curX));

                    float bezierFactor = !isTransition
                        ? Math.Sign(preX - postX) // preX < postX --> TrainDirection.ti
                        : 0.5f * Math.Sign(preX - curX); // Transition
                    var bezierOffset = new SizeF(bezierFactor * 14, (cp2.Y - cp1.Y) / -4.0f);
                    var bezierOffsetT = new SizeF(bezierOffset.Width, -bezierOffset.Height);

                    switch (attrs.StationLines)
                    {
                        case StationLineStyle.None:
                            p.MoveTo(cp2);
                            break;
                        case StationLineStyle.Normal:
                            p.AddLine(cp1, cp2);
                            break;
                        case StationLineStyle.Cubic:
                            var control2 = cp2 + (!isTransition ? bezierOffset : -1 * bezierOffsetT);
                            p.AddBezier(cp1, cp1 - bezierOffset, control2, cp2);
                            break;
                    }
                }
                else
                    p.AddLine(cp1, cp2); // Normal line between stations

                RenderTrainName(g, train, cp1, cp2, style, brush); // Zugnummern zeichnen.
            }
            g.DrawPath(pen, p);
        }

        private void RenderTrainName(Graphics g, ITrain train, PointF cp1, PointF cp2, TrainStyle style, Brush brush)
        {
            // only add the train number text, if the train line is not vertical or horizontal.
            if (Math.Abs(cp1.X - cp2.X) < TOLERANCE || Math.Abs(cp1.Y - cp2.Y) < TOLERANCE)
                return;
            var trainFont = (Font) attrs.TrainFont;

            var size = g.MeasureString(trainFont, train.TName);
            if (Math.Abs(cp1.X - cp2.X) < size.Width + 5) // the train name is wider than the current space in the diagram.
                return;

            float[] ys = { cp1.Y, cp2.Y };
            float[] xs = { cp1.X, cp2.X };
            var t = new PointF(xs.Average(), ys.Average());
            var d = new PointF(cp2.X - cp1.X, cp2.Y - cp1.Y); // train line vector.
            var n = new SizeF(d.Y, -d.X) / (float) Math.Sqrt(d.Y * d.Y + d.X * d.X); // normal vector to the train line (2d).
            t += Math.Sign(d.X) * n * (size.Height / 2 + style.CalcedWidth / 2f);

            if (t.Y < clipTop || t.Y > clipBottom) // check the clip area of the center, if this is outside we do not have to calc the angle.
                return;

            var angle = CalcAngle(ys, xs, train);
            var dh = (size.Width / 2 + size.Height / (2 + Math.Tan(angle))) * Math.Sin(angle);

            if (t.Y < clipTop + dh || t.Y > clipBottom - dh) // now check that we are fully inside the clipping area.
                return;

            var matrix = g.Transform.Clone();
            g.TranslateTransform(t.X, t.Y);
            g.RotateTransform(-angle);
            g.DrawText(trainFont, brush, -(size.Width / 2), -(size.Height / 2), train.TName);
            g.Transform = matrix;
        }

        #region Render helpers
        private float GetTimeY(TimeEntry time) => margin.Top + ((time - startTime).GetTotalMinutes() * attrs.HeightPerHour / 60f);

        PointF? GetGutterPoint(bool arrival, bool dir, StationRenderProps sx, TimeEntry time)
        {
            if (time == default)
                return null;
            var x = arrival ^ dir ? sx.Left : sx.Right;
            return new PointF(margin.Left + x, GetTimeY(time));
        }

        PointF? GetInternalPoint(StationRenderProps sx, TimeEntry time, string track)
        {
            if (time == default || track == null || !sx.TrackOffsets.TryGetValue(track, out float x))
                return null;
            return new PointF(margin.Left + x, GetTimeY(time));
        }

        void MaybeAddPoint(List<PointF> points, PointF? point)
        {
            if (point.HasValue)
                points.Add(point.Value);
        }

        (PointF cp1, PointF cp2) GetClippedPointsForLine(PointF p1, PointF p2)
        {
            var dx = p2.X - p1.X;
            var dy = p2.Y - p1.Y;
            if (p1.Y > clipTop && p2.Y < clipBottom)
                return (p1, p2);
            if (p1.Y > clipTop && p2.Y > clipBottom)
            {
                var clippedy = clipBottom - p2.Y;
                var x = dy > TOLERANCE ? dx / dy * clippedy : 0;
                return (p1, new PointF(p2.X + x, clipBottom));
            }
            if (p1.Y < clipTop && p2.Y > clipBottom)
            {
                var clippedy = clipTop - p1.Y;
                var x = dy > TOLERANCE ? dx / dy * clippedy : 0;
                return (new PointF(p1.X + x, clipTop), p2);
            }

            throw new Exception("tried to get clipped points for path fully outside rendering area, this should not happen!");
        }
        #endregion

        #region Direction helpers
        private bool GetTrainDirection(ITrain train)
        {
            if (tt.Type == TimetableType.Linear)
                return train.Direction == TrainDirection.ta;

            return GetTrains(TrainDirection.ta).Contains(train);
        }

        private Station[] GetStationsInDir(TrainDirection dir)
        {
            var route = (tt.Type == TimetableType.Linear ? tt.GetRoute(Timetable.LINEAR_ROUTE_ID).Stations : stations)
                .ToArray(); // Kopie erzeugen
            if (dir.IsSortReverse())
                Array.Reverse(route);

            return route;
        }

        private ITrain[] GetTrains(TrainDirection dir)
        {
            if (trainCache == null)
            {
                var stasAfter = GetStationsInDir(dir);
                trainCache = tt.Trains.Where(t =>
                {
                    var p = t.GetPath();
                    var ardeps = t.GetArrDepsUnsorted();

                    var intersect = stasAfter.Intersect(p)
                        .Where(s => ardeps[s].HasMinOneTimeSet).ToArray();

                    if (!intersect.Any())
                        return false;

                    var time1 = ardeps[intersect.First()].FirstSetTime;
                    var time2 = ardeps[intersect.Last()].FirstSetTime;

                    return time1 < time2;
                }).ToArray();
            }
            return trainCache;
        }
        #endregion

        private float CalcAngle(float[] ys, float[] xs, ITrain train)
        {
            var angle = (float)(Math.Atan2(xs.Max() - xs.Min(), ys.Max() - ys.Min()) * (180d / Math.PI));
            return GetTrainDirection(train) ? 90 - angle : angle - 90;
        }

        private IEnumerable<Station> GetSortedStations(ITrain train)
        {
            var path = train.GetPath();
            var arrdeps = train.GetArrDepsUnsorted();
            foreach (var sta in path)
            {
                if (arrdeps.TryGetValue(sta, out var ar))
                    if (ar.HasMinOneTimeSet)
                        yield return sta;
            }
        }

        private const double TOLERANCE = 1e-3; // Numerical tolerance for float comparisons.
    }
}

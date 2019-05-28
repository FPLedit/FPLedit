using FPLedit.Bildfahrplan.Model;
using FPLedit.Shared;
using System;
using System.Collections.Generic;
using Eto.Drawing;
using System.Linq;

namespace FPLedit.Bildfahrplan.Render
{
    internal class TrainRenderer
    {
        private readonly List<Station> stations; // Selected route, ordered
        private readonly Timetable tt;
        private Margins margin = new Margins(10, 20, 20, 20);
        private readonly TimetableStyle attrs;
        private readonly TimeSpan startTime;
        private readonly Dictionary<Station, StationX> stationOffsets;
        private Train[] trainCache;

        private readonly DashStyleHelper ds = new DashStyleHelper();

        public TrainRenderer(List<Station> stations, Timetable tt, Margins margin, TimeSpan startTime, Dictionary<Station, StationX> stationOffsets)
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
            var style = new TrainStyle(train, attrs);
            if (!style.CalcedShow)
                return;

            var ardps = train.GetArrDeps();
            var dir = GetTrainDirection(train);

            var pen = new Pen((Color)style.CalcedColor, style.CalcedWidth)
            {
                DashStyle = ds.ParseDashstyle(style.CalcedLineStyle)
            };
            var brush = new SolidBrush((Color)style.CalcedColor);

            List<PointF> points = new List<PointF>();
            bool hadFirstArrival = false, hadLastDeparture = false, isFirst = true;
            var stas = dir ? Enumerable.Reverse(stations) : stations;

            // Render helpers
            float GetTimeY(TimeSpan time) => margin.Top + ((time - startTime).GetMinutes() * attrs.HeightPerHour / 60f);

            PointF? GetGutterPoint(bool arrival, StationX sx, TimeSpan time)
            {
                if (time == default)
                    return null;
                var x = arrival ^ dir ? sx.Left : sx.Right;
                return new PointF(margin.Left + x, GetTimeY(time));
            }

            PointF? GetInternalPoint(StationX sx, TimeSpan time, string track)
            {
                if (time == default || track == null || !sx.TrackOffsets.TryGetValue(track, out float x))
                    return null;
                return new PointF(margin.Left + x, GetTimeY(time));
            }

            void MaybeAddPoint(PointF? point)
            {
                if (point.HasValue)
                    points.Add(point.Value);
            }

            foreach (var sta in stas)
            {
                if (!ardps.ContainsKey(sta))
                    continue;
                var ardp = ardps[sta];

                if (!ardp.HasMinOneTimeSet)
                    continue;

                MaybeAddPoint(GetGutterPoint(true, stationOffsets[sta], ardp.Arrival));
                MaybeAddPoint(GetInternalPoint(stationOffsets[sta], ardp.Arrival, ardp.ArrivalTrack));

                foreach (var shunt in ardp.ShuntMoves)
                {
                    MaybeAddPoint(GetInternalPoint(stationOffsets[sta], shunt.Time, shunt.SourceTrack));
                    MaybeAddPoint(GetInternalPoint(stationOffsets[sta], shunt.Time, shunt.TargetTrack));
                }

                MaybeAddPoint(GetInternalPoint(stationOffsets[sta], ardp.Departure, ardp.DepartureTrack));
                MaybeAddPoint(GetGutterPoint(false, stationOffsets[sta], ardp.Departure));

                hadLastDeparture = ardp.Departure != default;
                if (isFirst)
                    hadFirstArrival = ardp.Arrival != default;
                isFirst = false;
            }

            // Halbe Linien bei Abfahrten / Ankünften ohne Gegenstelle
            var hly = !dir ? 20 : -20;
            if (hadLastDeparture)
                points.Add(points.Last() + new Size(50, hly));
            if (hadFirstArrival)
                points.Insert(0, points.First() - new Size(50, hly));

            // Verbindung zum Folgezug
            var transition = tt.GetTransition(train);
            if (transition != null && !hadLastDeparture && attrs.StationLines != StationLineStyle.None)
            {
                var lastStaOfFirst = GetSortedStations(train)?.LastOrDefault();
                var firstStaOfNext = GetSortedStations(transition)?.FirstOrDefault();

                if (lastStaOfFirst == firstStaOfNext)
                {
                    var departure = transition.GetArrDep(firstStaOfNext).Departure;
                    points.Add(new PointF(points.Last().X, GetTimeY(departure)));
                }
            }

            var p = new GraphicsPath();
            for (int i = 0; i < points.Count; i += 1)
            {
                if (points.Count <= i + 1)
                    continue;

                var isStationLine = (int)points[i].X == (int)points[i + 1].X;
                if (isStationLine)
                {
                    var preX = i > 0 ? points[i - 1].X : 0;
                    var postX = i < points.Count - 2 ? points[i + 2].X : 0;
                    var x = points[i].X;
                    var isTransition = isStationLine && (points.Count == i + 2 || Math.Sign(preX - x) == Math.Sign(postX - x));

                    float bezierFactor = !isTransition ?
                        ((preX < postX) ? -1 : 1) : // preX < postX --> TrainDirection.ti
                        Math.Sign(preX - x); // Bei Transitions
                    if (isTransition) bezierFactor *= 0.5f;
                    var bezierOffset = new SizeF(bezierFactor * 14, (points[i + 1].Y - points[i].Y) / -4.0f);
                    var bezierOffsetT = new SizeF(bezierOffset.Width, -bezierOffset.Height);

                    switch (attrs.StationLines)
                    {
                        case StationLineStyle.None:
                            p.MoveTo(points[i + 1]);
                            break;
                        case StationLineStyle.Normal:
                            p.AddLine(points[i], points[i + 1]);
                            break;
                        case StationLineStyle.Cubic:
                            var control2 = points[i + 1] + (!isTransition ? bezierOffset : -bezierOffsetT);
                            p.AddBezier(points[i], points[i] - bezierOffset, control2, points[i + 1]);
                            break;
                    }
                }
                else
                    p.AddLine(points[i], points[i + 1]); // Normale Zuglinie

                if (points[i].X == points[i + 1].X || points[i].Y == points[i + 1].Y)
                    continue;
                // Zugnummern zeichnen
                var size = g.MeasureString((Font)attrs.TrainFont, train.TName);
                float[] ys = new[] { points[i].Y, points[i + 1].Y };
                float[] xs = new[] { points[i].X, points[i + 1].X };
                float ty = ys.Min() + (ys.Max() - ys.Min()) / 2 - (size.Height / 2);
                float tx = xs.Min() + (xs.Max() - xs.Min()) / 2;

                float angle = CalcAngle(ys, xs, train);
                g.SaveTransform();
                g.TranslateTransform(tx, ty);
                g.RotateTransform(-angle);
                g.DrawText((Font)attrs.TrainFont, brush, -(size.Width / 2), -(size.Height / 2), train.TName);
                g.RestoreTransform();
            }
            g.DrawPath(pen, p);
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
            if (trainCache == null)
            {
                var stasAfter = GetStationsInDir(dir);
                trainCache = tt.Trains.Where(t =>
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
            return trainCache;
        }
        #endregion

        private float CalcAngle(float[] ys, float[] xs, Train train)
        {
            float angle = (float)(Math.Atan2(xs.Max() - xs.Min(), ys.Max() - ys.Min()) * (180d / Math.PI));
            return GetTrainDirection(train) ? 90 - angle : angle - 90;
        }

        private IEnumerable<Station> GetSortedStations(Train train)
        {
            var path = train.GetPath();
            var arrdeps = train.GetArrDeps();
            foreach (var sta in path)
            {
                if (arrdeps[sta].HasMinOneTimeSet)
                    yield return sta;
            }
        }
    }
}

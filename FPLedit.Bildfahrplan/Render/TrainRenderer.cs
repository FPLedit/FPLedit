﻿using FPLedit.Bildfahrplan.Model;
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

        private readonly DashStyleHelper ds = new DashStyleHelper();

        public TrainRenderer(IEnumerable<Station> stations, Timetable tt, Margins margin, TimeEntry startTime, Dictionary<Station, StationRenderProps> stationOffsets, Days renderDays)
        {
            this.stations = stations;
            this.tt = tt;
            this.margin = margin;
            this.startTime = startTime;
            this.stationOffsets = stationOffsets;
            this.renderDays = renderDays;
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

                var isStationLine = (int)points[i].X == (int)points[i + 1].X;
                if (isStationLine)
                {
                    var preX = i > 0 ? points[i - 1].X : 0;
                    var postX = i < points.Count - 2 ? points[i + 2].X : 0;
                    var x = points[i].X;
                    var isTransition = isStationLine && (points.Count == i + 2 || Math.Sign(preX - x) == Math.Sign(postX - x));

                    float bezierFactor = !isTransition ?
                        ((preX < postX) ? -1 : 1) : // preX < postX --> TrainDirection.ti
                        Math.Sign(preX - x); // Transition
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
                            var control2 = points[i + 1] + (!isTransition ? bezierOffset : (SizeF.Empty-bezierOffsetT));
                            p.AddBezier(points[i], points[i] - bezierOffset, control2, points[i + 1]);
                            break;
                    }
                }
                else
                    p.AddLine(points[i], points[i + 1]); // Normal line between stations

                if (points[i].X == points[i + 1].X || points[i].Y == points[i + 1].Y)
                    continue;
                // Zugnummern zeichnen
                var trainFont = (Font)attrs.TrainFont;

                var size = g.MeasureString(trainFont, train.TName);
                float[] ys = { points[i].Y, points[i + 1].Y };
                float[] xs = { points[i].X, points[i + 1].X };
                float ty = ys.Min() + (ys.Max() - ys.Min()) / 2 - (size.Height / 2);
                float tx = xs.Min() + (xs.Max() - xs.Min()) / 2;

                if (g.Clip.IsVisible(new PointF(tx, ty))) // translated drawing does not respect clip (Idk why)
                {
                    float angle = CalcAngle(ys, xs, train);
                            
                    var matrix = g.Transform.Clone();
                            
                    g.TranslateTransform(tx, ty);
                    g.RotateTransform(-angle);
                    g.DrawText(trainFont, brush, -(size.Width / 2), -(size.Height / 2), train.TName);
                            
                    g.Transform = matrix;
                }
            }
            g.DrawPath(pen, p);
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
            if (tt.Type == TimetableType.Linear)
                return tt.GetLinearStationsOrderedByDirection(dir).ToArray();

            var route = stations.ToList(); // Kopie erzeugen
            if (dir == TrainDirection.ta)
                route.Reverse();

            return route.ToArray();
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
                        .Where(s => ardeps[s].HasMinOneTimeSet);

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
    }
}

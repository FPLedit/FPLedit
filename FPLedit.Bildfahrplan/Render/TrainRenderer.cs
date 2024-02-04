using FPLedit.Bildfahrplan.Model;
using FPLedit.Shared;
using FPLedit.Shared.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using FPLedit.Bildfahrplan.Helpers;
using FPLedit.Shared.Helpers;

namespace FPLedit.Bildfahrplan.Render;

internal sealed class TrainRenderer
{
    private readonly IList<Station> stations; // Selected route, ordered
    private readonly Timetable tt;
    private readonly Margins margin;
    private readonly TimetableStyle attrs;
    private readonly TimeEntry startTime;
    private readonly Dictionary<Station, StationRenderProps> stationOffsets;
    private readonly Days renderDays;

    private readonly DashStyleHelper ds = new();
    private readonly TrackHelper tracks = new();

    private readonly float clipTop, clipBottom, clipRight, clipLeft;

    public TrainRenderer(IList<Station> stations, Timetable tt, Margins margin, TimeEntry startTime, Dictionary<Station, StationRenderProps> stationOffsets, Days renderDays, float clipTop, float clipBottom, float clipRight)
    {
        this.stations = stations;
        this.tt = tt;
        this.margin = margin;
        this.startTime = startTime;
        this.stationOffsets = stationOffsets;
        this.renderDays = renderDays;
        this.clipTop = clipTop;
        this.clipBottom = clipBottom;
        this.clipLeft = 0;
        this.clipRight = clipRight;
        attrs = new TimetableStyle(tt);
    }

    public void Render(IMGraphics g, ITrain train)
    {
        var style = new TrainStyle(train, attrs);
        if (!style.CalcedShow)
            return;

        var pathData = new TrainPathData(train.ParentTimetable, train);
        var ardps = train.GetArrDepsUnsorted();
        var path = train.GetPath();

        Vec2 HalfLineVec(bool dir) => new (dir ? -50 : 50, 20); // Halbe Linien bei Abfahrten / Ankünften ohne Gegenstelle

        var pen = (style.CalcedColor, style.CalcedWidth, ds.ParseDashstyle(style.CalcedLineStyle));
        var brush = style.CalcedColor;

        List<Vec2> points = new List<Vec2>();
        List<int> pointBreaks = new List<int>();
        bool? hadFirstArrival = null, hadLastDeparture = null;
        var isFirst = true;

        Station? lastSta = null;
        var hadLastSkippedStation = false;
        var trainTravelsRouteCount = 0;
        foreach (var sta in path)
        {
            var skippedStation = lastSta != null &&
                                 (!stations.Contains(lastSta)
                                  || (stations.Contains(sta) && Math.Abs(stations.IndexOf(lastSta) - stations.IndexOf(sta)) != 1));

            if (skippedStation && !hadLastSkippedStation && points.Count > 0)
            {
                // Halbe Linien bei Abfahrten / Ankünften ohne Gegenstelle (hier innerhalb der Strecke und nicht am Rand)
                if (attrs.DrawNetworkTrains && hadLastDeparture.HasValue)
                    points.Add(points.Last() + HalfLineVec(hadLastDeparture.Value));
                pointBreaks.Add(points.Count);
            }
            hadLastSkippedStation = !isFirst && skippedStation;

            if (!stations.Contains(sta))
            {
                lastSta = sta;
                continue;
            }

            var ardp = ardps[sta];
            trainTravelsRouteCount++;
            if (!ardp.HasMinOneTimeSet)
            {
                lastSta = sta;
                continue;
            }

            var prePointIndex = points.Count; // needed for prepending a half-line

            // This is the geometrical "direction" on the 1D projection shown on this timetable (_for this segment_), not any "data-model" direction...
            bool dir;
            var curOffset = stationOffsets[sta];
            if (!skippedStation && !isFirst)
            {
                stationOffsets.TryGetValue(lastSta ?? sta, out var lastOffset);
                dir = (lastOffset ?? curOffset).Center > curOffset.Center;
            }
            else
            {
                var staIdx = path.IndexOf(sta) + 1;
                var nextSta = path.Count <= staIdx ? null : path[staIdx];
                stationOffsets.TryGetValue(nextSta ?? sta, out var nextOffset);
                dir = curOffset.Center > (nextOffset ?? curOffset).Center;
            }

            MaybeAddPoint(points, GetGutterPoint(true, dir, curOffset, ardp.Arrival));
            var arrivalTrack = tracks.GetTrack(pathData, sta, null, ardp, TrackQuery.Arrival);
            MaybeAddPoint(points, GetInternalPoint(curOffset, ardp.Arrival, arrivalTrack));

            foreach (var shunt in ardp.ShuntMoves)
            {
                MaybeAddPoint(points, GetInternalPoint(curOffset, shunt.Time, shunt.SourceTrack));
                MaybeAddPoint(points, GetInternalPoint(curOffset, shunt.Time, shunt.TargetTrack));
            }

            var departureTrack = tracks.GetTrack(pathData, sta, null, ardp, TrackQuery.Departure);
            MaybeAddPoint(points, GetInternalPoint(curOffset, ardp.Departure, departureTrack));
            MaybeAddPoint(points, GetGutterPoint(false, dir, curOffset, ardp.Departure));

            // Halbe Linien bei Abfahrten / Ankünften ohne Gegenstelle (hier innerhalb der Strecke und nicht am Rand)
            if (skippedStation && !isFirst && attrs.DrawNetworkTrains && ardp.Arrival != default)
                points.Insert(prePointIndex, points[prePointIndex] - HalfLineVec(dir));

            hadLastDeparture = ardp.Departure != default ? dir : null;
            if (isFirst)
                hadFirstArrival = ardp.Arrival != default ? dir : null;
            isFirst = false;

            lastSta = sta;
        }

        // Halbe Linien bei Abfahrten / Ankünften ohne Gegenstelle
        if (attrs.DrawNetworkTrains)
        {
            if (hadLastDeparture.HasValue)
                points.Add(points.Last() + HalfLineVec(hadLastDeparture.Value));

            if (hadFirstArrival.HasValue)
            {
                pointBreaks = pointBreaks.Select(pb => pb + 1).ToList(); // increment all break indices, as we will prepend a point.
                points.Insert(0, points.First() - HalfLineVec(hadFirstArrival.Value));
            }
        }
        else if (trainTravelsRouteCount <= 1)
            return; // This train has only one station on this route and we don't draw network trains.

        if (points.Count == 0)
            return; // This train is not travelling on this route

        // Transition to the next train; filtered by days and station.
        var lastStaOfFirst = GetSortedStations(train)?.LastOrDefault();
        var transition = tt.GetTransition(train, renderDays, lastStaOfFirst);
        if (transition != null && !(hadLastDeparture.HasValue && hadLastDeparture.Value) && attrs.StationLines != StationLineStyle.None && transition.Days.IsIntersecting(renderDays))
        {
            var firstStaOfNext = GetSortedStations(transition)?.FirstOrDefault();

            if (lastStaOfFirst == firstStaOfNext && firstStaOfNext != null)
            {
                var departure = transition.GetArrDep(firstStaOfNext).Departure;
                points.Add(new Vec2(points.Last().X, GetTimeY(departure)));
            }
        }

        var p = new List<IPathCmd>();

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
            if (!InsideClipping(points[i], points[i + 1]))
                continue;
            var (cp1, cp2) = GetClippedPointsForLine(points[i], points[i + 1]);

            if (pointBreaks.Contains(i + 1))
            {
                p.Add(new PathMoveCmd(cp2));
                continue;
            }

            if (isStationLine) // Line in one station.
            {
                var preX = i > 0 ? points[i - 1].X : 0;
                var postX = i < points.Count - 2 ? points[i + 2].X : 0;
                var curX = points[i].X; // explicitely don't use op1 for state calculations.
                var isTransition = points.Count == i + 2 || Math.Sign(preX - curX) == Math.Sign(postX - curX);

                float bezierFactor = !isTransition
                    ? Math.Sign(preX - postX) // preX < postX --> TrainDirection.ti
                    : 0.5f * Math.Sign(preX - curX); // Transition
                var bezierOffset = new Vec2(bezierFactor * 14, (cp2.Y - cp1.Y) / -4.0f);

                switch (attrs.StationLines)
                {
                    case StationLineStyle.None:
                        p.Add(new PathMoveCmd(cp2));
                        break;
                    case StationLineStyle.Normal:
                        p.Add(new PathLineCmd(cp1, cp2));
                        break;
                    case StationLineStyle.Cubic:
                        var control2 = cp2 + (!isTransition ? bezierOffset : -1 * bezierOffset.Transpose);
                        p.Add(new PathBezierCmd(cp1, cp1 - bezierOffset, control2, cp2));
                        break;
                }
            }
            else
            {
                p.Add(new PathLineCmd(cp1, cp2)); // Normal line between stations
                RenderTrainName(g, train, cp1, cp2, style, brush); // Zugnummern zeichnen.
            }
        }
        g.DrawPath(pen, p);
    }

    private void RenderTrainName(IMGraphics g, ITrain train, Vec2 cp1, Vec2 cp2, TrainStyle style, MColor brush)
    {
        // only add the train number text, if the train line is not vertical or horizontal.
        if (Math.Abs(cp1.X - cp2.X) < TOLERANCE || Math.Abs(cp1.Y - cp2.Y) < TOLERANCE)
            return;
        var trainFont = attrs.TrainFont;

        var size = g.MeasureString(trainFont, train.TName);
        if (Math.Abs(cp1.X - cp2.X) < size.Width + 5) // the train name is wider than the current space in the diagram.
            return;

        float[] ys = { cp1.Y, cp2.Y };
        float[] xs = { cp1.X, cp2.X };
        var t = new Vec2(xs.Average(), ys.Average());
        var d = new Vec2(cp2.X - cp1.X, cp2.Y - cp1.Y); // train line vector.
        var n = new Vec2(d.Y, -d.X) / (float) Math.Sqrt(d.Y * d.Y + d.X * d.X); // normal vector to the train line (2d).
        t += n * Math.Sign(d.X) *(size.Height / 2 + style.CalcedWidth / 2f);

        if (t.Y < clipTop || t.Y > clipBottom) // check the clip area of the center, if this is outside we do not have to calc the angle.
            return;
        if (t.X < clipLeft + margin.Left || t.X > clipRight - margin.Right)
            return;

        var angle = CalcAngle(ys, xs);
        var dh = (size.Width / 2 + size.Height / (2 + Math.Tan(angle))) * Math.Sin(angle);

        if (t.Y < clipTop + dh || t.Y > clipBottom - dh) // now check that we are fully inside the clipping area.
            return;
        // currently, do not check for detailed clipping in X direction.

        var matrix = g.StoreTransform();
        g.TranslateTransform(t.X, t.Y);
        g.RotateTransform(-angle);
        g.DrawText(trainFont, brush, -(size.Width / 2), -(size.Height / 2), train.TName);
        g.RestoreTransform(matrix);
    }

    #region Render helpers
    private float GetTimeY(TimeEntry time) => margin.Top + ((time - startTime).GetTotalMinutes() * attrs.HeightPerHour / 60f);

    Vec2? GetGutterPoint(bool arrival, bool dir, StationRenderProps sx, TimeEntry time)
    {
        if (time == default)
            return null;
        var x = arrival ^ dir ? sx.Left : sx.Right;
        return new Vec2(margin.Left + x, GetTimeY(time));
    }

    Vec2? GetInternalPoint(StationRenderProps sx, TimeEntry time, string? track)
    {
        if (time == default || track == null || !sx.TrackOffsets.TryGetValue(track, out var x))
            return null;
        return new Vec2(margin.Left + x, GetTimeY(time));
    }

    void MaybeAddPoint(List<Vec2> points, Vec2? point)
    {
        if (point.HasValue)
            points.Add(point.Value);
    }

    bool InsideClipping(Vec2 p1, Vec2 p2) => (p1.Y >= clipTop && p2.Y <= clipBottom) || (p1.Y <= clipTop && p2.Y >= clipBottom) || (p1.Y >= clipTop && p2.Y >= clipBottom);

    (Vec2 cp1, Vec2 cp2) GetClippedPointsForLine(Vec2 p1, Vec2 p2)
    {
        var dx = p2.X - p1.X;
        var dy = p2.Y - p1.Y;
        if (p1.Y >= clipTop && p2.Y <= clipBottom)
            return (p1, p2);
        if (p1.Y >= clipTop && p2.Y >= clipBottom)
        {
            var clippedy = clipBottom - p2.Y;
            var x = dy > TOLERANCE ? dx / dy * clippedy : 0;
            return (p1, new Vec2(p2.X + x, clipBottom));
        }
        if (p1.Y <= clipTop && p2.Y >= clipBottom)
        {
            var clippedy = clipTop - p1.Y;
            var x = dy > TOLERANCE ? dx / dy * clippedy : 0;
            return (new Vec2(p1.X + x, clipTop), p2);
        }

        throw new Exception("tried to get clipped points for path fully outside rendering area, this should not happen!");
    }
    #endregion

    private float CalcAngle(float[] ys, float[] xs)
    {
        var angle = (float)(Math.Atan2(xs[0] - xs[1], ys[0] - ys[1]) * (180d / Math.PI));
        return angle < 90 ? angle + 90 : angle - 90;
    }

    private IEnumerable<Station>? GetSortedStations(ITrain train)
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
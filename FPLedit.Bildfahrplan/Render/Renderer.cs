using System;
using FPLedit.Bildfahrplan.Model;
using FPLedit.Shared;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;

namespace FPLedit.Bildfahrplan.Render
{
    internal sealed class Renderer
    {
        private readonly Timetable tt;
        private readonly Func<PathData> getPathData;

        private Margins defaultMargin = new Margins(10, 20, 20, 20);
        private readonly Margins deafultHeaderMargin = new Margins(11, 20, 20, 0);

        private readonly TimetableStyle attrs;

        public Renderer(Timetable timetable, Func<PathData> getPathData)
        {
            tt = timetable;
            this.getPathData = getPathData;
            attrs = new TimetableStyle(tt);
        }

        public void SetMargins(Margins margins)
        {
            defaultMargin = margins;
        }

        public void Draw(Graphics g, bool drawHeader, float? forceWidth = null, bool exportColor = false)
            => Draw(g, attrs.StartTime, GetEndTime(attrs.StartTime, attrs.EndTime), drawHeader, forceWidth, exportColor);

        public static Func<PathData> DefaultPathData(int route, Timetable tt) => () => tt.GetRoute(route).ToPathData(tt);

        public void Draw(Graphics g, TimeEntry startTime, TimeEntry endTime, bool drawHeader, float? forceWidth = null, bool exportColor = false)
        {
            g.Clear(attrs.BgColor.ToSD(exportColor));

            var path = getPathData();
            var stations = path.GetRawPath().ToList();

            var margin = CalcMargins(g, defaultMargin, stations, startTime, endTime, drawHeader);
            var width = forceWidth ?? g.ClipBounds.Width;
            var height = GetHeight(g, startTime, endTime, drawHeader);

            // Zeitaufteilung
            var timeRenderer = new TimeRenderer(attrs);
            timeRenderer.Render(g, margin, startTime, endTime, width, exportColor);

            // Stationenaufteilung
            var headerRenderer = new HeaderRenderer(attrs, path);
            var stationOffsets = headerRenderer.Render(g, margin, width, height, drawHeader, exportColor);

            // Züge
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.SetClip(new RectangleF(0 /*margin.Left*/, margin.Top, width /*- margin.Left - margin.Right*/, height - margin.Bottom - margin.Top));

            var trains = tt.Trains.Where(t => t.Days.IsIntersecting(attrs.RenderDays));
            var trainRenderer = new TrainRenderer(stations, tt, margin, startTime, stationOffsets, attrs.RenderDays);
            foreach (var train in trains)
                trainRenderer.Render(g, train, exportColor);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.Default;
        }

        public void DrawHeader(Graphics g, int width, bool exportColor)
        {
            g.Clear(attrs.BgColor.ToSD(exportColor));

            var path = getPathData();
            var stations = path.GetRawPath().ToList();

            var margin = CalcMargins(g, deafultHeaderMargin, stations, attrs.StartTime, GetEndTime(attrs.StartTime, attrs.EndTime), true);

            var height = GetHeight(g, TimeEntry.Zero, TimeEntry.Zero, true); // Draw empty timespan.

            // Stationenaufteilung
            var headerRenderer = new HeaderRenderer(attrs, path);
            headerRenderer.Render(g, margin, width, height, true, exportColor);
        }

        private Margins CalcMargins(Graphics g, Margins orig, IEnumerable<Station> stations, TimeEntry startTime, TimeEntry endTime, bool drawHeader)
        {
            const int additionalMargin = 5;
            var result = new Margins(orig.Left + additionalMargin, orig.Top + additionalMargin, orig.Right + additionalMargin, orig.Bottom + additionalMargin);
            
            var path = getPathData();

            // MarginTop berechnen
            var hr = new HeaderRenderer(attrs, path);
            result.Top = drawHeader ? (
                attrs.DrawHeader ? 
                    hr.GetMarginTop(g) + result.Top 
                    : result.Top)
                : 5;

            // MarginLeft berechnen
            var tr = new TimeRenderer(attrs);
            result.Left += tr.GetMarginLeftOffset(g, startTime, endTime);
            
            return result;
        }

        public int GetHeightExternal(TimeEntry start, TimeEntry end, bool drawHeader)
        {
            using (var image = new Bitmap(1, 1, PixelFormat.Format24bppRgb))
            using (var g = Graphics.FromImage(image))
                return GetHeight(g, start, end, drawHeader);
        }
        
        public int GetHeight(Graphics g, TimeEntry start, TimeEntry end, bool drawHeader)
        {
            var stations = getPathData().GetRawPath().ToList();
            var m = CalcMargins(g, defaultMargin, stations, start, end, drawHeader);
            return (int)(m.Top + m.Bottom + (end - start).GetTotalMinutes() * attrs.HeightPerHour / 60f);
        }

        public int GetHeightExternal(bool drawHeader)
            => GetHeightExternal(attrs.StartTime, GetEndTime(attrs.StartTime, attrs.EndTime), drawHeader);

        private TimeEntry GetEndTime(TimeEntry startTime, TimeEntry endTime)
            => endTime < startTime ? endTime + new TimeEntry(24, 0) : endTime;
    }
}

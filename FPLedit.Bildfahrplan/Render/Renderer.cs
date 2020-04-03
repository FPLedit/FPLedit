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
        private readonly int route;

        private Margins defaultMargin = new Margins(10, 20, 20, 20);
        private readonly Margins deafultHeaderMargin = new Margins(11, 20, 20, 0);

        private readonly TimetableStyle attrs;

        public Renderer(Timetable timetable, int route)
        {
            tt = timetable;
            this.route = route;
            attrs = new TimetableStyle(tt);
        }

        public void SetMargins(Margins margins)
        {
            defaultMargin = margins;
        }

        public void Draw(Graphics g, bool drawHeader, float? forceWidth = null)
            => Draw(g, attrs.StartTime, attrs.EndTime, drawHeader, forceWidth);

        public void Draw(Graphics g, TimeEntry startTime, TimeEntry endTime, bool drawHeader, float? forceWidth = null)
        {
            g.Clear((Color)attrs.BgColor);

            var stations = tt.GetRoute(route).Stations;

            var margin = CalcMargins(g, defaultMargin, stations, startTime, endTime, drawHeader);
            var width = forceWidth ?? g.ClipBounds.Width;
            var height = GetHeight(g, startTime, endTime, drawHeader);

            // Zeitaufteilung
            var timeRenderer = new TimeRenderer(attrs);
            timeRenderer.Render(g, margin, startTime, endTime, width);

            // Stationenaufteilung
            var headerRenderer = new HeaderRenderer(stations, attrs, route);
            var stationOffsets = headerRenderer.Render(g, margin, width, height, drawHeader);

            // Züge
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.SetClip(new RectangleF(margin.Left, margin.Top, width - margin.Left - margin.Right, height - margin.Bottom - margin.Top));

            var trains = tt.Trains.Where(t =>
            {
                for (int i = 0; i < 6; i++)
                    if (attrs.RenderDays[i] && t.Days[i]) return true;
                return false;
            });
            var trainRenderer = new TrainRenderer(stations, tt, margin, startTime, stationOffsets);
            foreach (var train in trains)
                trainRenderer.Render(g, train);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.Default;
        }

        public void DrawHeader(Graphics g, int width)
        {
            g.Clear((Color)attrs.BgColor);

            var stations = tt.GetRoute(route).Stations;

            var margin = CalcMargins(g, deafultHeaderMargin, stations, attrs.StartTime, attrs.EndTime, true);

            var height = GetHeight(g, TimeEntry.Zero, TimeEntry.Zero, true); // Draw empty timespan.

            // Stationenaufteilung
            var headerRenderer = new HeaderRenderer(stations, attrs, route);
            headerRenderer.Render(g, margin, width, height, true);
        }

        private Margins CalcMargins(Graphics g, Margins orig, IEnumerable<Station> stations, TimeEntry startTime, TimeEntry endTime, bool drawHeader)
        {
            const int additionalMargin = 5;
            var result = new Margins(orig.Left + additionalMargin, orig.Top + additionalMargin, orig.Right + additionalMargin, orig.Bottom + additionalMargin);
            
            // MarginTop berechnen
            var hr = new HeaderRenderer(stations, attrs, route);
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
            var stations = tt.GetRoute(route).Stations;
            var m = CalcMargins(g, defaultMargin, stations, start, end, drawHeader);
            return (int)(m.Top + m.Bottom + (end - start).GetTotalMinutes() * attrs.HeightPerHour / 60f);
        }

        public int GetHeightExternal(bool drawHeader)
            => GetHeightExternal(attrs.StartTime, attrs.EndTime, drawHeader);
    }
}

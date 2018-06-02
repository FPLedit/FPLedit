using FPLedit.Bildfahrplan.Model;
using FPLedit.Shared;
using FPLedit.Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FPLedit.Bildfahrplan.Render
{
    public class Renderer
    {
        private Timetable tt;
        private int route;

        internal const string TIME_FORMAT = @"hh\:mm";

        private bool marginsCalced = false;
        private Margins margin = new Margins(10, 20, 20, 20);
        public float width = 0, height = 0;

        private TimetableStyle attrs;

        public Renderer(Timetable timetable, int route)
        {
            tt = timetable;
            this.route = route;
            attrs = new TimetableStyle(tt);
        }

        public void Draw(Graphics g)
            => Draw(g, attrs.StartTime, attrs.EndTime);

        public void Draw(Graphics g, TimeSpan startTime, TimeSpan endTime, int maxHeight = -1)
        {
            g.Clear(attrs.BgColor);

            if (width == 0) width = g.VisibleClipBounds.Width;
            if (height == 0) height = GetHeight(startTime, endTime);

            var stations = tt.GetRoute(route).GetOrderedStations();

            if (!marginsCalced)
                margin = CalcMargins(g, margin, stations, startTime, endTime);
            marginsCalced = true;

            // Zeitaufteilung
            var timeRenderer = new TimeRenderer(attrs, this);
            timeRenderer.Render(g, margin, startTime, endTime, width);

            // Stationenaufteilung
            var headerRenderer = new HeaderRenderer(stations, attrs, route);
            var stationOffsets = headerRenderer.Render(g, margin, width, height);

            // Züge
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            //TODO: Hier eine bessere Lösung, die nicht auf magic numbers basiert
            g.ExcludeClip(new Rectangle(0, 0, (int)width, (int)margin.Top)); // Kopf nicht bemalbar
            g.ExcludeClip(new Rectangle(0, GetHeight(startTime, endTime) - (int)margin.Bottom + 1, (int)width, (int)margin.Bottom + 20000)); // Unterer Rand nicht bemalbar (20000: Konstante für Page Margins)

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

        private Margins CalcMargins(Graphics g, Margins orig, IEnumerable<Station> stations, TimeSpan startTime, TimeSpan endTime)
        {
            var result = orig;
            // MarginTop berechnen
            float sMax = 0f;
            if (attrs.StationVertical)
                sMax = stations.Max(sta => g.MeasureString(sta.ToString(attrs.DisplayKilometre, route), attrs.StationFont).Width);
            else
                sMax = g.MeasureString("M", attrs.StationFont).Height;
            result.Top = attrs.DrawHeader ? sMax + result.Top : result.Top;

            // MarginLeft berechnen
            List<float> tsizes = new List<float>();
            foreach (var l in GetTimeLines(out bool h, startTime, endTime))
                tsizes.Add(g.MeasureString(new TimeSpan(0, l + startTime.GetMinutes(), 0).ToString(TIME_FORMAT), attrs.TimeFont).Width);
            result.Left = tsizes.Max() + result.Left;
            return result;
        }

        internal List<int> GetTimeLines(out bool hourStart, TimeSpan start, TimeSpan end)
        {
            List<int> lines = new List<int>();
            int minutesToNextLine = 60 - start.Minutes;
            if (minutesToNextLine == 60)
                lines.Add(0);
            if (minutesToNextLine >= 30)
                lines.Add(minutesToNextLine - 30);
            hourStart = lines.Count != 1;
            int min = 0;
            while (true)
            {
                min += minutesToNextLine;
                if (min > end.GetMinutes() - start.GetMinutes())
                    break;
                lines.Add(min);
                minutesToNextLine = 30;
            }
            return lines;
        }

        public int GetHeight(TimeSpan start, TimeSpan end)
        {
            var stations = tt.GetRoute(route).GetOrderedStations();
            using (var image = new Bitmap(1, 1))
            using (var g = Graphics.FromImage(image))
            {
                var m = CalcMargins(g, margin, stations, start, end);
                return (int)(m.Top + m.Bottom + (end - start).GetMinutes() * attrs.HeightPerHour / 60f);
            }
        }

        public int GetHeight()
            => GetHeight(attrs.StartTime, attrs.EndTime);
    }
}

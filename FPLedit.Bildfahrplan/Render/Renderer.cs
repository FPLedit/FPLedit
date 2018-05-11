using FPLedit.BildfahrplanExport.Model;
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

namespace FPLedit.BildfahrplanExport.Render
{
    public class Renderer
    {
        private Timetable tt;
        private int route;

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

            if (!marginsCalced)
            {
                // MarginTop berechnen
                float sMax = 0f;
                if (attrs.StationVertical)
                    sMax = tt.Stations.Max(sta => g.MeasureString(sta.ToString(attrs.DisplayKilometre, route), attrs.StationFont).Width);
                else
                    sMax = g.MeasureString("M", attrs.StationFont).Height;
                margin.Top = attrs.DrawHeader ? sMax + margin.Top : margin.Top;

                // MarginLeft berechnen
                List<float> tsizes = new List<float>();
                foreach (var l in GetTimeLines(startTime, endTime))
                    tsizes.Add(g.MeasureString(new TimeSpan(0, l + startTime.GetMinutes(), 0).ToString(@"hh\:mm"), attrs.TimeFont).Width);
                margin.Left = tsizes.Max() + margin.Left;
                marginsCalced = true;
            }

            // Zeitaufteilung
            foreach (var l in GetTimeLines(out bool hour, startTime, endTime))
            {
                var offset = margin.Top + l * attrs.HeightPerHour / 60f;
                g.DrawLine(new Pen(attrs.TimeColor, hour ? attrs.HourTimeWidth : attrs.MinuteTimeWidth), margin.Left - 5, offset, width - margin.Right, offset); // Linie

                var size = g.MeasureString(new TimeSpan(0, l + startTime.GetMinutes(), 0).ToString(@"hh\:mm"), attrs.TimeFont);
                g.DrawString(new TimeSpan(0, l + startTime.GetMinutes(), 0).ToString(@"hh\:mm"), attrs.TimeFont, new SolidBrush(attrs.TimeColor), margin.Left - 5 - size.Width, offset - (size.Height / 2)); // Beschriftung
                hour = !hour;
            }

            //TODO: Hier eine bessere Lösung, die nicht auf magic numbers basiert
            g.ExcludeClip(new Rectangle(0, GetHeight(startTime, endTime) - (int)margin.Bottom + 1, (int)width, (int)margin.Bottom + 20000)); // Unterer Rand nicht bemalbar (20000: Konstante für Page Margins)

            // Stationenaufteilung
            var stations = tt.GetRoute(route).GetOrderedStations();
            var firstStation = stations.First();
            var lastStation = stations.Last();
            var stationOffsets = new Dictionary<Station, float>();

            var verticalFormat = new StringFormat(StringFormatFlags.DirectionVertical);
            var stationBrush = new SolidBrush(attrs.StationColor);

            foreach (var sta in stations)
            {
                var kil = sta.Positions.GetPosition(route) - firstStation.Positions.GetPosition(route);
                var length = lastStation.Positions.GetPosition(route) - firstStation.Positions.GetPosition(route);

                if (!kil.HasValue || !length.HasValue)
                    throw new Exception("Unerwarteter Fehler beim Rendern der Route!");

                var pos = ((kil / length) * (width - margin.Right - margin.Left)).Value;
                var size = g.MeasureString(sta.ToString(attrs.DisplayKilometre, route), attrs.StationFont);

                g.DrawLine(new Pen(attrs.StationColor, attrs.StationWidth), margin.Left + pos, margin.Top - 5, margin.Left + pos, height - margin.Bottom); // Linie

                // Stationsnamen
                if (attrs.DrawHeader)
                {
                    var display = sta.ToString(attrs.DisplayKilometre, route);
                    if (attrs.StationVertical)
                        g.DrawString(display, attrs.StationFont, stationBrush, margin.Left + pos - (size.Height / 2), margin.Top - 5 - size.Width, verticalFormat);
                    else
                        g.DrawString(display, attrs.StationFont, stationBrush, margin.Left + pos - (size.Width / 2), margin.Top - size.Height - 5);
                }
                stationOffsets.Add(sta, pos);
            }

            // Züge
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.ExcludeClip(new Rectangle(0, 0, (int)width, (int)margin.Top)); // Kopf nicht bemalbar
            var trains = tt.Trains.Where(t =>
            {
                return (attrs.RenderDays[0] && t.Days[0]) || (attrs.RenderDays[1] && t.Days[1])
                || (attrs.RenderDays[2] && t.Days[2]) || (attrs.RenderDays[3] && t.Days[3])
                || (attrs.RenderDays[4] && t.Days[4]) || (attrs.RenderDays[5] && t.Days[5])
                || (attrs.RenderDays[6] && t.Days[6]);
            });
            var trainRenderer = new TrainRenderer(stations, tt, margin, startTime, stationOffsets);
            foreach (var train in trains)
                trainRenderer.Render(g, train);
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.Default;
        }

        private List<int> GetTimeLines(TimeSpan start, TimeSpan end)
            => GetTimeLines(out bool h, start, end);

        private List<int> GetTimeLines(out bool hourStart, TimeSpan start, TimeSpan end)
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
            using (var image = new Bitmap(1, 1))
            using (var g = Graphics.FromImage(image))
            {
                List<float> ssizes = new List<float>();
                foreach (var sta in tt.GetRoute(route).Stations)
                    ssizes.Add(g.MeasureString(sta.ToString(attrs.DisplayKilometre, route), attrs.StationFont).Width);

                if (!marginsCalced)
                    return (int)((attrs.DrawHeader ? ssizes.Max() : 0) + margin.Top + margin.Bottom + (end - start).GetMinutes() * attrs.HeightPerHour / 60f);
                else
                    return (int)(margin.Top + margin.Bottom + (end - start).GetMinutes() * attrs.HeightPerHour / 60f);
            }
        }

        public int GetHeight()
            => GetHeight(attrs.StartTime, attrs.EndTime);
    }
}

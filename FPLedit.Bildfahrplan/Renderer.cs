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

namespace FPLedit.BildfahrplanExport
{
    public class Renderer
    {
        private Timetable tt;
        private int route;

        private StringFormat format = new StringFormat(StringFormatFlags.DirectionVertical); //TODO: Konfigurierbar

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

        public void Draw(Graphics g, TimeSpan startTime, TimeSpan endTime)
        {
            g.Clear(attrs.BgColor);

            if (width == 0) width = g.VisibleClipBounds.Width;
            if (height == 0) height = GetHeight();

            if (!marginsCalced)
            {
                // MarginTop berechnen
                List<float> ssizes = new List<float>();
                foreach (var sta in tt.Stations)
                    ssizes.Add(g.MeasureString(sta.ToString(attrs.DisplayKilometre, route), attrs.StationFont).Width);
                margin.Top = attrs.DrawHeader ? ssizes.Max() + margin.Top : margin.Top;

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

            g.ExcludeClip(new Rectangle(0, GetHeight() - (int)margin.Bottom, (int)width, (int)margin.Bottom + 200)); // Unterer Rand nicht bemalbar (200: Konstante für Page Margins)

            // Stationenaufteilung
            var stations = tt.GetRoute(route).GetOrderedStations();
            var firstStation = stations.First();
            var lastStation = stations.Last();
            var stationOffsets = new Dictionary<Station, float>();

            foreach (var sta in stations)
            {
                var kil = sta.Positions.GetPosition(route) - firstStation.Positions.GetPosition(route);
                var length = lastStation.Positions.GetPosition(route) - firstStation.Positions.GetPosition(route);

                if (!kil.HasValue || !length.HasValue)
                    throw new Exception("Unerwarteter Fehler beim Rendern der Route!");

                var pos = ((kil / length) * (width - margin.Right - margin.Left)).Value;
                var size = g.MeasureString(sta.ToString(attrs.DisplayKilometre, route), attrs.StationFont);

                g.DrawLine(new Pen(attrs.StationColor, attrs.StationWidth), margin.Left + pos, margin.Top - 5, margin.Left + pos, height - margin.Bottom); // Linie

                if (attrs.DrawHeader)
                    g.DrawString(sta.ToString(attrs.DisplayKilometre, route), attrs.StationFont, new SolidBrush(attrs.StationColor), margin.Left + pos - (size.Height / 2), margin.Top - 5 - size.Width, format); // Beschriftung

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
            foreach (var train in trains)
            {
                if (!train.GetAttribute("sh", true))
                    continue;

                var style = new TrainStyle(train);
                var ardps = train.GetArrDeps();

                int tWidth = style.TrainWidth ?? attrs.TrainWidth;
                List<PointF> points = new List<PointF>();
                foreach (var sta in stations)
                {
                    PointF? arPoint = null, dpPoint = null;

                    if (!ardps.ContainsKey(sta))
                        continue;
                    var ardp = ardps[sta];

                    if (ardp.Departure != new TimeSpan())
                    {
                        float x = margin.Left + stationOffsets[sta];
                        TimeSpan timeOffset = ardp.Departure - startTime;
                        float y = margin.Top + timeOffset.GetMinutes() * attrs.HeightPerHour / 60f;
                        arPoint = new PointF(x, y);
                    }
                    if (ardp.Arrival != new TimeSpan())
                    {
                        float x = margin.Left + stationOffsets[sta];
                        TimeSpan timeOffset = ardp.Arrival - startTime;
                        float y = margin.Top + timeOffset.GetMinutes() * attrs.HeightPerHour / 60f;
                        dpPoint = new PointF(x, y);
                    }
                    if (train.Direction == TrainDirection.ta)
                    {
                        if (arPoint.HasValue)
                            points.Add(arPoint.Value);
                        if (arPoint.HasValue && dpPoint.HasValue && arPoint.Value != dpPoint.Value && attrs.StationLines)
                            points.AddRange(new[] { arPoint.Value, dpPoint.Value });
                        if (dpPoint.HasValue)
                            points.Add(dpPoint.Value);
                    }
                    else
                    {
                        if (dpPoint.HasValue)
                            points.Add(dpPoint.Value);
                        if (arPoint.HasValue && dpPoint.HasValue && arPoint.Value != dpPoint.Value && attrs.StationLines)
                            points.AddRange(new[] { arPoint.Value, dpPoint.Value });
                        if (arPoint.HasValue)
                            points.Add(arPoint.Value);
                    }
                }

                for (int i = 0; i < points.Count; i += 2)
                {
                    if (points.Count <= i + 1)
                        continue;

                    g.DrawLine(new Pen(style.TrainColor ?? attrs.TrainColor, tWidth), points[i], points[i + 1]);

                    if (points[i].X == points[i + 1].X)
                        continue;
                    var size = g.MeasureString(train.TName, attrs.TrainFont);
                    float[] yps = new[] { points[i].Y, points[i + 1].Y };
                    float[] xs = new[] { points[i].X, points[i + 1].X };
                    float y = yps.Min() + (yps.Max() - yps.Min()) / 2 - (size.Height / 2);
                    float x = xs.Min() + (xs.Max() - xs.Min()) / 2;

                    float angle = (float)(Math.Atan2(xs.Max() - xs.Min(), yps.Max() - yps.Min()) * (180d / Math.PI));
                    angle = GetTrainDirection(train) ? 90 - angle : angle - 90;
                    var container = g.BeginContainer();
                    g.TranslateTransform(x, y);
                    g.RotateTransform(-angle);
                    g.DrawString(train.TName, attrs.TrainFont, new SolidBrush(style.TrainColor ?? attrs.TrainColor), -(size.Width / 2), -(size.Height / 2));
                    g.EndContainer(container);
                }
            }
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

        private bool GetTrainDirection(Train train)
        {
            if (tt.Type == TimetableType.Linear)
                return train.Direction == TrainDirection.ta;

            var taTrains = GetTrains(TrainDirection.ta);
            return taTrains.Contains(train);
        }

        public Station[] GetStationsInDir(TrainDirection dir)
        {
            if (tt.Type == TimetableType.Linear)
                return tt.GetStationsOrderedByDirection(dir).ToArray();

            var route = tt.GetRoute(this.route).GetOrderedStations();
            if (dir == TrainDirection.ta)
                route.Reverse();

            return route.ToArray();
        }

        public Train[] GetTrains(TrainDirection dir)
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

                return time1 > time2;
            }).ToArray();
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

        public TimeSpan GetTimeByHeight(TimeSpan start, int height)
        {
            var end = start + new TimeSpan(0, 60 - start.Minutes, 0); // to full hour
            var one = new TimeSpan(1, 0, 0);
            TimeSpan last = end;
            int h = GetHeight(start, end);
            while (true)
            {
                end += one;
                h += attrs.HeightPerHour;
                if (h >= height)
                {
                    end = last;
                    var meta = attrs.EndTime;
                    if (end > meta)
                    {
                        end = meta;
                        return meta;
                    }
                    return last;
                }
                last = end;
            }
        }
    }
}

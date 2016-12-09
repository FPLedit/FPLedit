using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPLedit.BildfahrplanExport
{
    public class Renderer
    {
        Timetable tt;

        public Font stationFont = new Font("Arial", 9);
        public Font timeFont = new Font("Arial", 9);
        public Font trainFont = new Font("Arial", 9);
        public StringFormat format = new StringFormat(StringFormatFlags.DirectionVertical); //
        public int heightPerHour = 100;
        public TimeSpan startTime = new TimeSpan(0, 0, 0);
        public TimeSpan endTime = new TimeSpan(24, 0, 0);
        public Color backgroundColor = Color.White;
        public Color stationColor = Color.Black;
        public int stationWidth = 1;
        public Color timeColor = Color.Orange;
        public int hourTimeWidth = 2;
        public int minuteTimeWidth = 1;
        public Color trainColor = Color.Gray;
        public int trainWidth = 1;
        public bool stationLines = true;
        public bool[] days = new bool[7];
        public bool displayKilometre = true;
        public bool drawHeader = true;

        private bool marginsCalced = false;
        public float marginRight = 20;
        public float marginBottom = 20;
        public float marginTop = 20;
        public float marginLeft = 10;
        public float width = 0, height = 0;

        public Renderer(Timetable timetable)
        {
            tt = timetable;
            for (int i = 0; i < days.Length; i++)
                days[i] = true;
            Init();
        }

        public void Init()
        {
            trainColor = tt.GetMetaColor("TrainColor", trainColor);
            timeColor = tt.GetMetaColor("TimeColor", timeColor);
            backgroundColor = tt.GetMetaColor("BgColor", backgroundColor);
            stationColor = tt.GetMetaColor("StationColor", stationColor);
            trainWidth = tt.GetMetaInt("TrainWidth", trainWidth);
            stationWidth = tt.GetMetaInt("StationWidth", stationWidth);
            hourTimeWidth = tt.GetMetaInt("HourTimeWidth", hourTimeWidth);
            minuteTimeWidth = tt.GetMetaInt("MinuteTimeWidth", minuteTimeWidth);

            stationLines = tt.GetMetaBool("StationLines", stationLines);
            displayKilometre = tt.GetMetaBool("DisplayKilometre", displayKilometre);

            heightPerHour = tt.GetMetaInt("HeightPerHour", heightPerHour);
            days = tt.GetMeta("ShowDays", days, Train.ParseDays);
            startTime = tt.GetMetaTimeSpan("StartTime", startTime);
            endTime = tt.GetMeta("EndTime", endTime, s => TimeSpan.Parse(s == "24:00" ? "1.00:00" : s));

            stationFont = new Font(tt.GetMeta("StationFont", stationFont.Name), tt.GetMetaFloat("StationFontSize", stationFont.Size));
            timeFont = new Font(tt.GetMeta("TimeFont", timeFont.Name), tt.GetMetaFloat("TimeFontSize", timeFont.Size));
            trainFont = new Font(tt.GetMeta("TrainFont", trainFont.Name), tt.GetMetaFloat("TrainFontSize", trainFont.Size));

            drawHeader = tt.GetMetaBool("DrawHeader", drawHeader);
        }

        public void Draw(Graphics g)
        {
            g.Clear(backgroundColor);

            if (width == 0) width = g.VisibleClipBounds.Width;
            if (height == 0) height = GetHeight();

            if (!marginsCalced)
            {
                // MarginTop berechnen
                List<float> ssizes = new List<float>();
                foreach (var sta in tt.Stations)
                    ssizes.Add(g.MeasureString(sta.ToString(displayKilometre), stationFont).Width);
                marginTop = drawHeader ? ssizes.Max() + marginTop : marginTop;

                // MarginLeft berechnen
                List<float> tsizes = new List<float>();
                foreach (var l in GetTimeLines())
                    tsizes.Add(g.MeasureString(new TimeSpan(0, l + startTime.GetMinutes(), 0).ToString(@"hh\:mm"), timeFont).Width);
                marginLeft = tsizes.Max() + marginLeft;
                marginsCalced = true;
            }
            
            // Zeitaufteilung
            bool hour;
            foreach (var l in GetTimeLines(out hour))
            {
                var offset = marginTop + l * heightPerHour / 60f;
                g.DrawLine(new Pen(timeColor, hour ? hourTimeWidth : minuteTimeWidth), marginLeft - 5, offset, width - marginRight, offset); // Linie

                var size = g.MeasureString(new TimeSpan(0, l + startTime.GetMinutes(), 0).ToString(@"hh\:mm"), timeFont);
                g.DrawString(new TimeSpan(0, l + startTime.GetMinutes(), 0).ToString(@"hh\:mm"), timeFont, new SolidBrush(timeColor), marginLeft - 5 - size.Width, offset - (size.Height / 2)); // Beschriftung
                hour = !hour;
            }

            g.ExcludeClip(new Rectangle(0, GetHeight() - (int)marginBottom, (int)width, (int)marginBottom + 200)); // Unterer Rand nicht bemalbar (200: Konstante für Page Margins)

            // Stationenaufteilung
            var stations = tt.Stations.OrderBy(s => s.Kilometre);
            var firstStation = stations.First();
            var lastStation = stations.Last();
            var stationOffsets = new Dictionary<Station, float>();

            foreach (var sta in stations)
            {
                var kil = sta.Kilometre - firstStation.Kilometre;
                var length = lastStation.Kilometre - firstStation.Kilometre;

                var pos = (kil / length) * (width - marginRight - marginLeft);
                var size = g.MeasureString(sta.ToString(displayKilometre), stationFont);

                g.DrawLine(new Pen(stationColor, stationWidth), marginLeft + pos, marginTop - 5, marginLeft + pos, height - marginBottom); // Linie

                if (drawHeader)
                    g.DrawString(sta.ToString(displayKilometre), stationFont, new SolidBrush(stationColor), marginLeft + pos - (size.Height / 2), marginTop - 5 - size.Width, format); // Beschriftung

                stationOffsets.Add(sta, pos);
            }

            // Züge
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.ExcludeClip(new Rectangle(0, 0, (int)width, (int)marginTop)); // Kopf nicht bemalbar
            var trains = tt.Trains.Where(t =>
            {
                return (days[0] && t.Days[0]) || (days[1] && t.Days[1])
                || (days[2] && t.Days[2]) || (days[3] && t.Days[3])
                || (days[4] && t.Days[4]) || (days[5] && t.Days[5])
                || (days[6] && t.Days[6]);
            });            
            foreach (var train in trains)
            {
                if (!train.GetMeta("Draw", true, bool.Parse))
                    continue;

                Color color = train.GetMetaColor("Color", trainColor);
                int tWidth = train.GetMetaInt("Width", trainWidth);
                List<PointF> points = new List<PointF>();
                foreach (var sta in stations)
                {
                    PointF? arPoint = null, dpPoint = null;
                    if (train.Departures.ContainsKey(sta))
                    {
                        var dp = train.Departures[sta];
                        float x = marginLeft + stationOffsets[sta];
                        TimeSpan timeOffset = dp - startTime;
                        float y = marginTop + timeOffset.GetMinutes() * heightPerHour / 60f;
                        arPoint = new PointF(x, y);
                    }
                    if (train.Arrivals.ContainsKey(sta))
                    {
                        var ar = train.Arrivals[sta];
                        float x = marginLeft + stationOffsets[sta];
                        TimeSpan timeOffset = ar - startTime;
                        float y = marginTop + timeOffset.GetMinutes() * heightPerHour / 60f;
                        dpPoint = new PointF(x, y);
                    }
                    if (train.Direction)
                    {
                        if (arPoint.HasValue)
                            points.Add(arPoint.Value);
                        if (arPoint.HasValue && dpPoint.HasValue && arPoint.Value != dpPoint.Value && stationLines)
                            points.AddRange(new[] { arPoint.Value, dpPoint.Value });
                        if (dpPoint.HasValue)
                            points.Add(dpPoint.Value);
                    }
                    else
                    {
                        if (dpPoint.HasValue)
                            points.Add(dpPoint.Value);
                        if (arPoint.HasValue && dpPoint.HasValue && arPoint.Value != dpPoint.Value && stationLines)
                            points.AddRange(new[] { arPoint.Value, dpPoint.Value });
                        if (arPoint.HasValue)
                            points.Add(arPoint.Value);
                    }
                }

                for (int i = 0; i < points.Count; i += 2)
                {
                    if (points.Count <= i + 1)
                        continue;

                    g.DrawLine(new Pen(color, tWidth), points[i], points[i + 1]);

                    if (points[i].X == points[i + 1].X)
                        continue;
                    var size = g.MeasureString(train.Name, trainFont);
                    float[] yps = new[] { points[i].Y, points[i + 1].Y };
                    float[] xs = new[] { points[i].X, points[i + 1].X };
                    float y = yps.Min() + (yps.Max() - yps.Min()) / 2 - (size.Height / 2);
                    float x = xs.Min() + (xs.Max() - xs.Min()) / 2;

                    float angle = (float)(Math.Atan2(xs.Max() - xs.Min(), yps.Max() - yps.Min()) * (180d / Math.PI));
                    angle = train.Direction ? 90 - angle : angle - 90;
                    g.TranslateTransform(x, y);
                    g.RotateTransform(-angle);
                    g.DrawString(train.Name, trainFont, new SolidBrush(color), -(size.Width / 2), -(size.Height / 2));
                    g.RotateTransform(angle);
                    g.TranslateTransform(-x, -y);
                }
            }
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.Default;            
        }

        private List<int> GetTimeLines()
        {
            bool h;
            return GetTimeLines(out h);
        }

        private List<int> GetTimeLines(out bool hourStart)
        {
            List<int> lines = new List<int>();
            int minutesToNextLine = 60 - startTime.Minutes;
            if (minutesToNextLine == 60)
                lines.Add(0);
            if (minutesToNextLine >= 30)
                lines.Add(minutesToNextLine - 30);
            hourStart = lines.Count != 1;
            int min = 0;
            while (true)
            {
                min += minutesToNextLine;
                if (min > endTime.GetMinutes() - startTime.GetMinutes())
                    break;
                lines.Add(min);
                minutesToNextLine = 30;
            }
            return lines;
        }

        public int GetHeight()
        {
            using (var image = new Bitmap(1, 1))
            using (var g = Graphics.FromImage(image))
            {
                List<float> ssizes = new List<float>();
                foreach (var sta in tt.Stations)
                    ssizes.Add(g.MeasureString(sta.ToString(displayKilometre), stationFont).Width);

                if (!marginsCalced)
                    return (int)((drawHeader ? ssizes.Max() : 0) + marginTop + marginBottom + (endTime - startTime).GetMinutes() * heightPerHour / 60f);
                else
                    return (int)(marginTop + marginBottom + (endTime - startTime).GetMinutes() * heightPerHour / 60f);
            }
        }

        public TimeSpan GetTimeByHeight(bool drawHeader, TimeSpan start, int height)
        {
            this.drawHeader = drawHeader;
            startTime = start;
            endTime = start + new TimeSpan(0, 60 - start.Minutes, 0); // to full hour
            TimeSpan one = new TimeSpan(1, 0, 0);
            TimeSpan last = endTime;
            int h = GetHeight();
            while (true)
            {
                endTime += one;
                h += heightPerHour;
                if (h >= height)
                {
                    endTime = last;
                    var meta = tt.GetMeta("EndTime", new TimeSpan(24, 0, 0), s => TimeSpan.Parse(s == "24:00" ? "1.00:00" : s));
                    if (endTime > meta)
                    {
                        endTime = meta;
                        return meta;
                    }
                    return last;
                }
                last = endTime;
            }
        }
    }
}

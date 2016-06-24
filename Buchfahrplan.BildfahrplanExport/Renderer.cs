using Buchfahrplan.Shared;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Buchfahrplan.BildfahrplanExport
{
    public class Renderer
    {
        Timetable tt;

        Font stationFont = new Font("Arial", 9);
        Font timeFont = new Font("Arial", 9);
        Font trainFont = new Font("Arial", 9);
        StringFormat format = new StringFormat(StringFormatFlags.DirectionVertical); //
        int heightPerHour = 100;
        TimeSpan startTime = new TimeSpan(0, 0, 0);
        TimeSpan endTime = new TimeSpan(24, 0, 0);
        Color backgroundColor = Color.White;
        Color stationColor = Color.Black;
        int stationWidth = 1;
        Color timeColor = Color.Orange;
        int hourTimeWidth = 2;
        int minuteTimeWidth = 1;
        Color trainColor = Color.Gray;
        int trainWidth = 1;
        bool stationLines = true;
        bool[] days = new bool[7];
        bool displayKilometre = true;

        public Renderer(Timetable timetable)
        {
            tt = timetable;
            for (int i = 0; i < days.Length; i++)
                days[i] = true;
            Init();
        }

        public void Init()
        {
            /*if (tt.Metadata.ContainsKey("TrainColor"))
                trainColor = Color.FromName(tt.Metadata["TrainColor"]);*/
            trainColor = Meta.GetMeta(tt, "TrainColor", trainColor, Color.FromName);
            /*if (tt.Metadata.ContainsKey("TimeColor"))
                timeColor = Color.FromName(tt.Metadata["TimeColor"]);*/
            timeColor = Meta.GetMeta(tt, "TimeColor", timeColor, Color.FromName);
            if (tt.Metadata.ContainsKey("BgColor"))
                backgroundColor = Color.FromName(tt.Metadata["BgColor"]);
            if (tt.Metadata.ContainsKey("StationColor"))
                stationColor = Color.FromName(tt.Metadata["StationColor"]);
            if (tt.Metadata.ContainsKey("TrainWidth"))
                trainWidth = int.Parse(tt.Metadata["TrainWidth"]);
            if (tt.Metadata.ContainsKey("StationWidth"))
                stationWidth = int.Parse(tt.Metadata["StationWidth"]);
            if (tt.Metadata.ContainsKey("HourTimeWidth"))
                hourTimeWidth = int.Parse(tt.Metadata["HourTimeWidth"]);
            if (tt.Metadata.ContainsKey("MinuteTimeWidth"))
                minuteTimeWidth = int.Parse(tt.Metadata["MinuteTimeWidth"]);
            if (tt.Metadata.ContainsKey("StationFont") && tt.Metadata.ContainsKey("StationFontSize"))
                stationFont = new Font(tt.Metadata["StationFont"], int.Parse(tt.Metadata["StationFontSize"]));
            if (tt.Metadata.ContainsKey("TimeFont") && tt.Metadata.ContainsKey("TimeFontSize"))
                timeFont = new Font(tt.Metadata["TimeFont"], int.Parse(tt.Metadata["TimeFontSize"]));
            if (tt.Metadata.ContainsKey("TrainFont") && tt.Metadata.ContainsKey("TrainFontSize"))
                trainFont = new Font(tt.Metadata["TrainFont"], int.Parse(tt.Metadata["TrainFontSize"]));
            if (tt.Metadata.ContainsKey("StationLines"))
                stationLines = bool.Parse(tt.Metadata["StationLines"]);
            if (tt.Metadata.ContainsKey("HeightPerHour"))
                heightPerHour = int.Parse(tt.Metadata["HeightPerHour"]);
            if (tt.Metadata.ContainsKey("ShowDays"))
                days = Train.ParseDays(tt.Metadata["ShowDays"]);
            if (tt.Metadata.ContainsKey("StartTime"))
                startTime = TimeSpan.Parse(tt.Metadata["StartTime"]);
            if (tt.Metadata.ContainsKey("EndTime"))
                endTime = TimeSpan.Parse(tt.Metadata["EndTime"] == "24:00" ? "1.00:00" : tt.Metadata["EndTime"]);
            if (tt.Metadata.ContainsKey("DisplayKilometre"))
                displayKilometre = bool.Parse(tt.Metadata["DisplayKilometre"]);
        }

        public void Draw(Graphics g)
        {
            g.Clear(backgroundColor);

            float width = g.VisibleClipBounds.Width;
            float height = g.VisibleClipBounds.Height;
            float marginRight = 20;
            float marginBottom = 20;
            float marginTop, marginLeft;

            // MarginTop berechnen
            List<float> ssizes = new List<float>();
            foreach (var sta in tt.Stations)
                ssizes.Add(g.MeasureString(sta.ToString(displayKilometre), stationFont).Width);
            marginTop = ssizes.Max() + 20;

            // MarginLeft berechnen
            List<float> tsizes = new List<float>();
            foreach (var l in GetTimeLines())
                tsizes.Add(g.MeasureString(new TimeSpan(0, l + startTime.GetMinutes(), 0).ToString(@"hh\:mm"), timeFont).Width);
            marginLeft = tsizes.Max() + 10;

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

                g.DrawString(sta.ToString(displayKilometre), stationFont, new SolidBrush(stationColor), marginLeft + pos - (size.Height / 2), marginTop - 5 - size.Width, format); // Beschriftung

                stationOffsets.Add(sta, pos);
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

            // Züge
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            var trains = tt.Trains.Where(t =>
            {
                return (days[0] && t.Days[0]) || (days[1] && t.Days[1])
                || (days[2] && t.Days[2]) || (days[3] && t.Days[3])
                || (days[4] && t.Days[4]) || (days[5] && t.Days[5])
                || (days[6] && t.Days[6]);
            });
            foreach (var train in trains)
            {
                Color color = train.Metadata.ContainsKey("Color") ? Color.FromName(train.Metadata["Color"]) : trainColor;
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
                    g.DrawLine(new Pen(color, trainWidth), points[i], points[i + 1]);

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
            bool a;
            return GetTimeLines(out a);
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

                return (int)(ssizes.Max() + 40 + (endTime - startTime).GetMinutes() * heightPerHour / 60f);
            }
        }
    }
}

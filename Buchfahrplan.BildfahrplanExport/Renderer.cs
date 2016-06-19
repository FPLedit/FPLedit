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
        Timetable timetable;

        Font fnt = new Font("Arial", 9);
        StringFormat format = new StringFormat(StringFormatFlags.DirectionVertical);
        int heightPerHour = 100;
        int startHour = 4;
        int endHour = 23;
        Color stationColor = Color.Black;
        Color backgroundColor = Color.White;
        Color timeColor = Color.Orange;
        Color trainColor = Color.Gray;
        bool stationLines = true;

        public Renderer(Timetable timetable)
        {
            this.timetable = timetable;
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
            foreach (var sta in timetable.Stations)
                ssizes.Add(g.MeasureString($"{sta.Name} [{sta.Kilometre}]", fnt).Width);
            marginTop = ssizes.Max() + 20;

            // MarginLeft berechnen
            List<float> tsizes = new List<float>();
            for (int i = startHour; i <= endHour; i++)
                tsizes.Add(g.MeasureString(i.ToString("##") + ":00", fnt).Width);
            marginLeft = tsizes.Max() + 10;

            // Stationenaufteilung
            var stations = timetable.Stations.OrderBy(s => s.Kilometre);
            var firstStation = stations.First();
            var lastStation = stations.Last();
            var stationOffsets = new Dictionary<Station, float>();

            foreach (var sta in stations)
            {
                var kil = sta.Kilometre - firstStation.Kilometre;
                var length = lastStation.Kilometre - firstStation.Kilometre;

                var pos = (kil / length) * (width - marginRight - marginLeft);
                var size = g.MeasureString($"{sta.Name} [{sta.Kilometre}]", fnt);

                g.DrawLine(new Pen(stationColor, 1f), marginLeft + pos, marginTop - 5, marginLeft + pos, height - marginBottom); // Linie

                g.DrawString($"{sta.Name} [{sta.Kilometre}]", fnt, new SolidBrush(stationColor), marginLeft + pos - (size.Height / 2), marginTop - 5 - size.Width, format); // Beschriftung

                stationOffsets.Add(sta, pos);
            }

            // Zeitaufteilung            
            for (int i = startHour; i <= endHour; i++)
            {
                var offset = marginTop + (i - startHour) * heightPerHour;
                g.DrawLine(new Pen(timeColor), marginLeft - 5, offset, width - marginRight, offset); // Linie

                var size = g.MeasureString(i.ToString("##") + ":00", fnt);
                g.DrawString(i.ToString("##") + ":00", fnt, new SolidBrush(timeColor), marginLeft - 5 - size.Width, offset - (size.Height / 2)); // Beschriftung
            }

            // Züge
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            foreach (var train in timetable.Trains)
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
                        DateTime timeOffset = dp - new TimeSpan(startHour, 0, 0);
                        float y = marginTop + (timeOffset.Hour + timeOffset.Minute / 60f) * heightPerHour;
                        arPoint = new PointF(x, y);
                    }
                    if (train.Arrivals.ContainsKey(sta))
                    {
                        var ar = train.Arrivals[sta];
                        float x = marginLeft + stationOffsets[sta];
                        DateTime timeOffset = ar - new TimeSpan(startHour, 0, 0);
                        float y = marginTop + (timeOffset.Hour + timeOffset.Minute / 60f) * heightPerHour;
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
                    g.DrawLine(new Pen(color, 2f), points[i], points[i + 1]);

                    if (points[i].X == points[i + 1].X)
                        continue;
                    var size = g.MeasureString(train.Name, fnt);
                    float[] yps = new[] { points[i].Y, points[i + 1].Y };
                    float[] xs = new[] { points[i].X, points[i + 1].X };
                    float y = yps.Min() + (yps.Max() - yps.Min()) / 2 - (size.Height / 2);
                    float x = xs.Min() + (xs.Max() - xs.Min()) / 2;

                    float angle = (float)(Math.Atan2(xs.Max() - xs.Min(), yps.Max() - yps.Min()) * (180d / Math.PI));
                    angle = train.Direction ? 90 - angle : angle - 90;
                    g.TranslateTransform(x, y);
                    g.RotateTransform(-angle);
                    g.DrawString(train.Name, fnt, new SolidBrush(color), -(size.Width / 2), -(size.Height / 2));
                    g.RotateTransform(angle);
                    g.TranslateTransform(-x, -y);
                }
            }
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.Default;
        }

        public int GetHeight()
        {
            using (var image = new Bitmap(1, 1))
            {
                using (var g = Graphics.FromImage(image))
                {
                    List<float> ssizes = new List<float>();
                    foreach (var sta in timetable.Stations)
                        ssizes.Add(g.MeasureString($"{sta.Name} [{sta.Kilometre}]", fnt).Width);

                    return (int)ssizes.Max() + 40 + (endHour - startHour) * heightPerHour;
                }
            }
        }
    }
}

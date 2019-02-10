using FPLedit.Bildfahrplan.Model;
using FPLedit.Shared;
using System;
using System.Collections.Generic;
using Eto.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPLedit.Bildfahrplan.Render
{
    internal class HeaderRenderer
    {
        private List<Station> stations;
        private int route;
        private TimetableStyle attrs;

        private DashStyleHelper ds = new DashStyleHelper();

        public HeaderRenderer(List<Station> stations, TimetableStyle attrs, int route)
        {
            this.stations = stations;
            this.route = route;
            this.attrs = attrs;
        }

        public Dictionary<Station, float> Render(Graphics g, Margins margin, float width, float height)
        {
            var firstStation = stations.First();
            var lastStation = stations.Last();
            var stationOffsets = new Dictionary<Station, float>();

            foreach (var sta in stations)
            {
                var style = new StationStyle(sta, attrs);

                var kil = sta.Positions.GetPosition(route) - firstStation.Positions.GetPosition(route);
                var length = lastStation.Positions.GetPosition(route) - firstStation.Positions.GetPosition(route);

                if (!kil.HasValue || !length.HasValue)
                    throw new Exception("Unerwarteter Fehler beim Rendern der Route!");

                var pos = ((kil / length) * (width - margin.Right - margin.Left)).Value;
                stationOffsets.Add(sta, pos);

                if (!style.CalcedShow)
                    continue;

                var pen = new Pen((Color)style.CalcedColor, style.CalcedWidth);
                pen.DashStyle = ds.ParseDashstyle(style.CalcedLineStyle);
                var brush = new SolidBrush((Color)style.CalcedColor);

                var size = g.MeasureString((Font)attrs.StationFont, sta.ToString(attrs.DisplayKilometre, route));

                g.DrawLine(pen, margin.Left + pos, margin.Top - 5, margin.Left + pos, height - margin.Bottom); // Linie

                // Stationsnamen
                if (attrs.DrawHeader)
                {
                    var display = sta.ToString(attrs.DisplayKilometre, route);

                    if (attrs.StationVertical)
                    {
                        g.SaveTransform();
                        g.TranslateTransform(margin.Left + pos + (size.Height / 2), margin.Top - 8 - size.Width);
                        g.RotateTransform(90);
                        g.DrawText((Font)attrs.StationFont, brush, 0, 0, display);
                        g.RestoreTransform();
                    }
                    else
                        g.DrawText((Font)attrs.StationFont, brush, margin.Left + pos - (size.Width / 2), margin.Top - size.Height - 5, display);
                }

            }
            return stationOffsets;
        }
    }
}

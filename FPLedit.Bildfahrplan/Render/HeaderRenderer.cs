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
        private readonly List<Station> stations;
        private readonly int route;
        private readonly TimetableStyle attrs;

        private readonly DashStyleHelper ds = new DashStyleHelper();

        public HeaderRenderer(List<Station> stations, TimetableStyle attrs, int route)
        {
            this.stations = stations;
            this.route = route;
            this.attrs = attrs;
        }

        public Dictionary<Station, StationX> Render(Graphics g, Margins margin, float width, float height, bool drawHeader)
        {
            var firstStation = stations.First();
            var lastStation = stations.Last();
            var stationOffsets = new Dictionary<Station, StationX>();

            var allTrackCount = stations.Select(s => s.Tracks.Count).Sum();
            var stasWithTracks = stations.Count(s => s.Tracks.Any());
            var allTrackWidth = (stasWithTracks + allTrackCount) * StationX.IndividualTrackOffset;

            var emSize = g.MeasureString((Font)attrs.StationFont, "M").Width;

            StationX lastPos = null;
            foreach (var sta in stations)
            {
                var style = new StationStyle(sta, attrs);

                var kil = sta.Positions.GetPosition(route) - firstStation.Positions.GetPosition(route);
                var length = lastStation.Positions.GetPosition(route) - firstStation.Positions.GetPosition(route);

                if (!kil.HasValue || !length.HasValue)
                    throw new Exception("Unerwarteter Fehler beim Rendern der Route!");

                StationX posX;
                if (!attrs.MultiTrack)
                    posX = new StationX(sta, kil.Value, ((kil / length) * (width - margin.Right - margin.Left)).Value);
                else
                {
                    var availWidth = width - margin.Right - margin.Left - allTrackWidth;
                    var lastKil = lastPos?.CurKilometer ?? 0f;
                    var lastRight = lastPos?.Right ?? 0f;
                    var leftOffset = (((kil / length) - (lastKil / length)) * availWidth).Value;
                    posX = new StationX(sta, kil.Value, lastRight + leftOffset, true);
                }
                lastPos = posX;
                stationOffsets.Add(sta, posX);

                if (!style.CalcedShow)
                    continue;

                var pen = new Pen((Color)style.CalcedColor, style.CalcedWidth)
                {
                    DashStyle = ds.ParseDashstyle(style.CalcedLineStyle)
                };
                var brush = new SolidBrush((Color)style.CalcedColor);

                if (!attrs.MultiTrack)
                {
                    // Linie (Single-Track-Mode)
                    g.DrawLine(pen, margin.Left + posX.Center, margin.Top - 5, margin.Left + posX.Center, height - margin.Bottom);
                }
                else
                {
                    // Linie (Multi-Track-Mode)
                    g.DrawLine(pen, margin.Left + posX.Left, margin.Top - 5, margin.Left + posX.Left, height - margin.Bottom);
                    foreach (var trackX in posX.TrackOffsets)
                        g.DrawLine(pen, margin.Left + trackX.Value, margin.Top - 5, margin.Left + trackX.Value, height - margin.Bottom);
                    g.DrawLine(pen, margin.Left + posX.Right, margin.Top - 5, margin.Left + posX.Right, height - margin.Bottom);
                }

                if (!drawHeader)
                    continue;

                // Stationsnamen
                if (attrs.DrawHeader)
                {
                    var display = sta.ToString(attrs.DisplayKilometre, route);
                    var size = g.MeasureString((Font)attrs.StationFont, display);

                    var addOffset = attrs.MultiTrack ? emSize + 3 : 0;

                    if (attrs.StationVertical)
                    {
                        g.SaveTransform();
                        g.TranslateTransform(margin.Left + posX.Center + (size.Height / 2), margin.Top - 8 - addOffset - size.Width);
                        g.RotateTransform(90);
                        g.DrawText((Font)attrs.StationFont, brush, 0, 0, display);
                        g.RestoreTransform();
                    }
                    else
                        g.DrawText((Font)attrs.StationFont, brush, margin.Left + posX.Center - (size.Width / 2), margin.Top - size.Height - addOffset - 5, display);

                    if (attrs.MultiTrack)
                    {
                        foreach (var track in posX.TrackOffsets)
                        {
                            var trackSize = g.MeasureString((Font)attrs.StationFont, track.Key);
                            g.DrawText((Font)attrs.StationFont, brush, margin.Left + track.Value - (trackSize.Width / 2), margin.Top - trackSize.Height - 5, track.Key);
                        }
                    }
                }

            }
            return stationOffsets;
        }
    }
}

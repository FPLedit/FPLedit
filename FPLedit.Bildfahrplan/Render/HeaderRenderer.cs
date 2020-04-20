using FPLedit.Bildfahrplan.Model;
using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace FPLedit.Bildfahrplan.Render
{
    internal sealed class HeaderRenderer
    {
        private readonly IEnumerable<Station> stations;
        private readonly int route;
        private readonly TimetableStyle attrs;

        private readonly DashStyleHelper ds = new DashStyleHelper();

        private const int TOP_GAP = 5;
        
        public HeaderRenderer(IEnumerable<Station> stations, TimetableStyle attrs, int route)
        {
            this.stations = stations;
            this.route = route;
            this.attrs = attrs;
        }

        public Dictionary<Station, StationRenderProps> Render(Graphics g, Margins margin, float width, float height, bool drawHeader)
        {
            var stationFont = (Font)attrs.StationFont; // Reminder: Do not dispose, will be disposed with MFont instance!
            var firstStation = stations.First();
            var lastStation = stations.Last();
            var stationOffsets = new Dictionary<Station, StationRenderProps>();

            var allTrackCount = stations.Select(s => s.Tracks.Count).Sum();
            var stasWithTracks = stations.Count(s => s.Tracks.Any());
            var allTrackWidth = (stasWithTracks + allTrackCount) * StationRenderProps.IndividualTrackOffset;
            var verticalTrackOffset = GetTrackOffset(g, stationFont) + TOP_GAP;

            StationRenderProps lastPos = null;
            foreach (var sta in stations)
            {
                var style = new StationStyle(sta, attrs);

                var kil = sta.Positions.GetPosition(route) - firstStation.Positions.GetPosition(route);
                var length = lastStation.Positions.GetPosition(route) - firstStation.Positions.GetPosition(route);

                if (!kil.HasValue || !length.HasValue)
                    throw new Exception("Unerwarteter Fehler beim Rendern der Route!");

                StationRenderProps posX;
                if (!attrs.MultiTrack)
                    posX = new StationRenderProps(sta, kil.Value, ((kil / length) * (width - margin.Right - margin.Left)).Value);
                else
                {
                    var availWidth = width - margin.Right - margin.Left - allTrackWidth;
                    var lastKil = lastPos?.CurKilometer ?? 0f;
                    var lastRight = lastPos?.Right ?? 0f;
                    var leftOffset = (((kil / length) - (lastKil / length)) * availWidth).Value;
                    posX = new StationRenderProps(sta, kil.Value, lastRight + leftOffset, true);
                }
                lastPos = posX;
                stationOffsets.Add(sta, posX);

                if (!style.CalcedShow)
                    continue;

                using (var pen = new Pen((Color)style.CalcedColor, style.CalcedWidth)
                {
                    DashPattern = ds.ParseDashstyle(style.CalcedLineStyle)
                })
                using (var brush = new SolidBrush((Color)style.CalcedColor))
                {
                    if (!attrs.MultiTrack)
                    {
                        // Linie (Single-Track-Mode)
                        g.DrawLine(pen, margin.Left + posX.Center, margin.Top - TOP_GAP, margin.Left + posX.Center, height - margin.Bottom);
                    }
                    else
                    {
                        // Linie (Multi-Track-Mode)
                        g.DrawLine(pen, margin.Left + posX.Left, margin.Top - TOP_GAP, margin.Left + posX.Left, height - margin.Bottom);
                        foreach (var trackX in posX.TrackOffsets)
                            g.DrawLine(pen, margin.Left + trackX.Value, margin.Top - TOP_GAP, margin.Left + trackX.Value, height - margin.Bottom);
                        g.DrawLine(pen, margin.Left + posX.Right, margin.Top - TOP_GAP, margin.Left + posX.Right, height - margin.Bottom);
                    }

                    if (!drawHeader)
                        continue;

                    // Stationsnamen
                    if (attrs.DrawHeader)
                    {
                        var display = StationDisplay(sta);
                        var size = g.MeasureString(stationFont, display);

                        if (attrs.StationVertical)
                        {
                            var matrix = g.Transform.Clone();
                            
                            g.TranslateTransform(margin.Left + posX.Center + (size.Height / 2), margin.Top - 8 - verticalTrackOffset - size.Width);
                            g.RotateTransform(90);
                            g.DrawText(stationFont, brush, 0, 0, display);

                            g.Transform = matrix;
                        }
                        else
                            g.DrawText(stationFont, brush, margin.Left + posX.Center - (size.Width / 2), margin.Top - size.Height - verticalTrackOffset - TOP_GAP, display);

                        if (attrs.MultiTrack)
                        {
                            foreach (var track in posX.TrackOffsets)
                            {
                                var trackSize = g.MeasureString(stationFont, track.Key);
                                if (attrs.StationVertical)
                                {
                                    var matrix = g.Transform.Clone();
                                    
                                    g.TranslateTransform(margin.Left + track.Value + (trackSize.Height / 2), margin.Top - 8 - trackSize.Width);
                                    g.RotateTransform(90);
                                    g.DrawText(stationFont, brush, 0, 0, track.Key);
                                    
                                    g.Transform = matrix;
                                }
                                else
                                    g.DrawText(stationFont, brush, margin.Left + track.Value - (trackSize.Width / 2), margin.Top - trackSize.Height - TOP_GAP, track.Key);
                            }
                        }
                    }
                } // Disposing Pens and Brushes
            }
            return stationOffsets;
        }

        private string StationDisplay(Station sta) => sta.ToString(attrs.DisplayKilometre, route) + (!string.IsNullOrWhiteSpace(sta.StationCode) ? $" ({sta.StationCode})" : "");

        public float GetMarginTop(Graphics g)
        {
            var stationFont = (Font)attrs.StationFont;
            var emSize = g.MeasureString(stationFont, "M").Height;
            
            var sMax = attrs.StationVertical ? 
                (stations.Any() ?
                    stations.Max(sta => g.MeasureString(StationDisplay(sta), stationFont).Width) 
                    : 0)
                : emSize;

            sMax += GetTrackOffset(g, stationFont) + TOP_GAP + 3;
            
            return sMax;
        }

        private float GetTrackOffset(Graphics g, Font stationFont)
        {
            var emSize = g.MeasureString(stationFont, "M").Height;
            if (attrs.MultiTrack)
            {
                var tracks = stations.SelectMany(s => s.Tracks);
                if (!tracks.Any())
                    return 0;
                
                if (attrs.StationVertical)
                    return tracks.Max(t => g.MeasureString(t.Name, stationFont).Width);

                return emSize;
            }

            return 0f;
        }
    }
}

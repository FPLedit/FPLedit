﻿using FPLedit.Bildfahrplan.Model;
using FPLedit.Shared;
using FPLedit.Shared.Rendering;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using FPLedit.Bildfahrplan.Helpers;

namespace FPLedit.Bildfahrplan.Render
{
    internal sealed class HeaderRenderer
    {
        private readonly PathData path;
        private readonly TimetableStyle attrs;

        private readonly DashStyleHelper ds = new DashStyleHelper();

        private const int TOP_GAP = 5;
        
        public HeaderRenderer(TimetableStyle attrs, PathData path)
        {
            this.path = path;
            this.attrs = attrs;
        }

        public Dictionary<Station, StationRenderProps> Render(Graphics g, Margins margin, float width, float height, bool drawHeader, bool exportColor)
        {
            var stationFont = (Font)attrs.StationFont; // Reminder: Do not dispose, will be disposed with MFont instance!
            var stationOffsets = new Dictionary<Station, StationRenderProps>();

            var raw = path.GetRawPath().ToList();
            var allTrackCount = raw.Select(s => s.Tracks.Count).Sum();
            var stasWithTracks = raw.Count(s => s.Tracks.Any());
            var allTrackWidth = (stasWithTracks + allTrackCount) * StationRenderProps.IndividualTrackOffset;
            var verticalTrackOffset = GetTrackOffset(g, stationFont) + TOP_GAP;

            float length = 0f;

            PathEntry lastpe = null;
            foreach (var sta in path.PathEntries)
            {
                var er = path.GetEntryRoute(sta.Station);
                if (er != Timetable.UNASSIGNED_ROUTE_ID)
                    length += (sta!.Station.Positions.GetPosition(er) - lastpe!.Station.Positions.GetPosition(er))!.Value;
                lastpe = sta;
            }

            StationRenderProps lastPos = null;
            lastpe = null;
            float kil = 0f;
            foreach (var sta in path.PathEntries)
            {
                var style = new StationStyle(sta.Station, attrs);
                
                var er = path.GetEntryRoute(sta.Station);
                if (er != Timetable.UNASSIGNED_ROUTE_ID)
                    kil += (sta!.Station.Positions.GetPosition(er) - lastpe!.Station.Positions.GetPosition(er))!.Value;
                lastpe = sta;
                
                StationRenderProps posX;
                if (!attrs.MultiTrack)
                    posX = new StationRenderProps(sta.Station, kil, ((kil / length) * (width - margin.Right - margin.Left)));
                else
                {
                    var availWidth = width - margin.Right - margin.Left - allTrackWidth;
                    var lastKil = lastPos?.CurKilometer ?? 0f;
                    var lastRight = lastPos?.Right ?? 0f;
                    var leftOffset = (((kil / length) - (lastKil / length)) * availWidth);
                    posX = new StationRenderProps(sta.Station, kil, lastRight + leftOffset, true);
                }
                lastPos = posX;
                stationOffsets.Add(sta.Station, posX);

                if (!style.CalcedShow)
                    continue;

                using var pen = new Pen(style.CalcedColor.ToSD(exportColor), style.CalcedWidth)
                {
                    DashPattern = ds.ParseDashstyle(style.CalcedLineStyle)
                };
                using var brush = new SolidBrush(style.CalcedColor.ToSD(exportColor));
                
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
            }
            return stationOffsets;
        }

        private string StationDisplay(PathEntry sta) => sta.Station.ToString(attrs.DisplayKilometre, sta.RouteIndex) + 
                                                        (!string.IsNullOrWhiteSpace(sta.Station.StationCode) ? $" ({sta.Station.StationCode})" : "");

        public float GetMarginTop(Graphics g)
        {
            var stationFont = (Font)attrs.StationFont;
            var emSize = g.MeasureString(stationFont, "M").Height;
            
            var sMax = attrs.StationVertical ? 
                (path.PathEntries.Any() ?
                    path.PathEntries.Max(sta => g.MeasureString(StationDisplay(sta), stationFont).Width) 
                    : 0)
                : emSize;

            sMax += GetTrackOffset(g, stationFont) + TOP_GAP + 3;
            
            return sMax;
        }

        private float GetTrackOffset(Graphics g, Font stationFont)
        {
            var raw = path.GetRawPath().ToList();
            var emSize = g.MeasureString(stationFont, "M").Height;
            if (attrs.MultiTrack)
            {
                var tracks = raw.SelectMany(s => s.Tracks);
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

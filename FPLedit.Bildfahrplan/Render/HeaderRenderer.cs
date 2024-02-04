using FPLedit.Bildfahrplan.Model;
using FPLedit.Shared;
using FPLedit.Shared.Rendering;
using System.Collections.Generic;
using System.Linq;
using FPLedit.Bildfahrplan.Helpers;

namespace FPLedit.Bildfahrplan.Render;

internal sealed class HeaderRenderer
{
    private readonly PathData path;
    private readonly TimetableStyle attrs;

    private readonly DashStyleHelper ds = new ();

    private const int TOP_GAP = 5;

    public HeaderRenderer(TimetableStyle attrs, PathData path)
    {
        this.path = path;
        this.attrs = attrs;
    }

    public Dictionary<Station, StationRenderProps> Render(IMGraphics g, Margins margin, float width, float height, bool drawHeader)
    {
        var stationOffsets = new Dictionary<Station, StationRenderProps>();

        var posAlongPath = path.GetPositionsAlongPath();
        var raw = path.GetRawPath();
        var allTrackCount = raw.Select(s => s.Tracks.Count).Sum();
        var stasWithTracks = raw.Count(s => s.Tracks.Any());
        var allTrackWidth = (stasWithTracks + allTrackCount) * StationRenderProps.IndividualTrackOffset;
        var verticalTrackOffset = GetTrackOffset(g, attrs.StationFont) + TOP_GAP;

        float length = posAlongPath.Values.Max();

        StationRenderProps? lastPos = null;
        foreach (var sta in path.PathEntries)
        {
            var style = new StationStyle(sta.Station, attrs);

            var kil = posAlongPath[sta.Station];

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

            var pen = (style.CalcedColor, style.CalcedWidth, ds.ParseDashstyle(style.CalcedLineStyle));
            var brush = style.CalcedColor;

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
                var size = g.MeasureString(attrs.StationFont, display);

                if (attrs.StationVertical)
                {
                    var matrix = g.StoreTransform();

                    g.TranslateTransform(margin.Left + posX.Center + (size.Height / 2), margin.Top - 8 - verticalTrackOffset - size.Width);
                    g.RotateTransform(90);
                    g.DrawText(attrs.StationFont, brush, 0, 0, display);

                    g.RestoreTransform(matrix);
                }
                else
                    g.DrawText(attrs.StationFont, brush, margin.Left + posX.Center - (size.Width / 2), margin.Top - size.Height - verticalTrackOffset - TOP_GAP, display);

                if (attrs.MultiTrack)
                {
                    foreach (var track in posX.TrackOffsets)
                    {
                        var trackSize = g.MeasureString(attrs.StationFont, track.Key);
                        if (attrs.StationVertical)
                        {
                            var matrix = g.StoreTransform();

                            g.TranslateTransform(margin.Left + track.Value + (trackSize.Height / 2), margin.Top - 8 - trackSize.Width);
                            g.RotateTransform(90);
                            g.DrawText(attrs.StationFont, brush, 0, 0, track.Key);

                            g.RestoreTransform(matrix);
                        }
                        else
                            g.DrawText(attrs.StationFont, brush, margin.Left + track.Value - (trackSize.Width / 2), margin.Top - trackSize.Height - TOP_GAP, track.Key);
                    }
                }
            }
        }
        return stationOffsets;
    }

    private string StationToString(Station sta, bool kilometre, int route)
        => sta.SName + (kilometre ? (" (" + sta.Positions.GetPosition(route) + ")") : "");

    private string StationDisplay(PathEntry sta) => StationToString(sta.Station, attrs.DisplayKilometre, sta.RouteIndex) +
                                                    (!string.IsNullOrWhiteSpace(sta.Station.StationCode) ? $" ({sta.Station.StationCode})" : "");

    public float GetMarginTop(IMGraphics g)
    {
        var emSize = g.MeasureString(attrs.StationFont, "M").Height;

        var sMax = attrs.StationVertical ?
            (path.PathEntries.Any() ?
                path.PathEntries.Max(sta => g.MeasureString(attrs.StationFont, StationDisplay(sta)).Width)
                : 0)
            : emSize;

        sMax += GetTrackOffset(g, attrs.StationFont) + TOP_GAP + 3;

        return sMax;
    }

    private float GetTrackOffset(IMGraphics g, MFont stationFont)
    {
        var raw = path.GetRawPath();
        var emSize = g.MeasureString(stationFont, "M").Height;
        if (attrs.MultiTrack)
        {
            var tracks = raw.SelectMany(s => s.Tracks);
            if (!tracks.Any())
                return 0;

            if (attrs.StationVertical)
                return tracks.Max(t => g.MeasureString(stationFont, t.Name).Width);

            return emSize;
        }

        return 0f;
    }
}
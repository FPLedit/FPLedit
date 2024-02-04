using System;
using FPLedit.Bildfahrplan.Model;
using System.Collections.Generic;
using System.Linq;
using FPLedit.Shared;
using FPLedit.Shared.Rendering;

namespace FPLedit.Bildfahrplan.Render;

internal sealed class TimeRenderer
{
    private readonly TimetableStyle attrs;

    public TimeRenderer(TimetableStyle attrs)
    {
        this.attrs = attrs;
    }

    public void Render(IMGraphics g, Margins margin, TimeEntry startTime, TimeEntry endTime, float width)
    {
        var minutePen = (attrs.TimeColor, attrs.MinuteTimeWidth, Array.Empty<float>());
        var hourPen = (attrs.TimeColor, attrs.HourTimeWidth, Array.Empty<float>());

        foreach (var l in GetTimeLines(out bool hour, startTime, endTime))
        {
            var offset = margin.Top + l * attrs.HeightPerHour / 60f;
            g.DrawLine(hour ? hourPen : minutePen, margin.Left - 5, offset, width - margin.Right, offset); // Linie

            var text = new TimeEntry(0, l + startTime.GetTotalMinutes()).Normalize().ToShortTimeString();
            var size = g.MeasureString(attrs.TimeFont, text);
            g.DrawText(attrs.TimeFont, attrs.TimeColor, margin.Left - 5 - size.Width, offset - (size.Height / 2), text); // Beschriftung
            hour = !hour;
        }
    }

    public float GetMarginLeftOffset(IMGraphics g, TimeEntry startTime, TimeEntry endTime)
    {
        return GetTimeLines(out _, startTime, endTime)
            .Select(l => g.MeasureString(attrs.TimeFont, (startTime + new TimeEntry(0, l)).Normalize().ToShortTimeString()).Width)
            .Concat(new[] { 0f }).Max();
    }

    private List<int> GetTimeLines(out bool isStartFullHour, TimeEntry start, TimeEntry end)
    {
        if (end < start) // prevent endless loop
        {
            isStartFullHour = false;
            return new List<int>();
        }

        List<int> lines = new List<int>();

        void MaybeAddLine(int minutesFromStart)
        {
            if (start + new TimeEntry(0, minutesFromStart) <= end)
                lines.Add(minutesFromStart);
        }

        int minutesToNextLine = 60 - start.Minutes;
        if (minutesToNextLine == 60)
            MaybeAddLine(0);
        if (minutesToNextLine >= 30)
            MaybeAddLine(minutesToNextLine - 30);
        isStartFullHour = lines.Count != 1;
        int min = 0;
        while (true)
        {
            min += minutesToNextLine;
            if (min > end.GetTotalMinutes() - start.GetTotalMinutes())
                break;
            MaybeAddLine(min);
            minutesToNextLine = 30;
        }
        return lines;
    }
}
using FPLedit.Bildfahrplan.Model;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using FPLedit.Shared;
using FPLedit.Shared.Rendering;

namespace FPLedit.Bildfahrplan.Render
{
    internal sealed class TimeRenderer
    {
        private readonly TimetableStyle attrs;

        public TimeRenderer(TimetableStyle attrs)
        {
            this.attrs = attrs;
        }

        public void Render(Graphics g, Margins margin, TimeEntry startTime, TimeEntry endTime, float width, bool exportColor)
        {
            var timeFont = (Font)attrs.TimeFont; // Reminder: Do not dispose, will be disposed with MFont instance!
            using var timeBrush = new SolidBrush(attrs.TimeColor.ToSD(exportColor));
            using var minutePen = new Pen(attrs.TimeColor.ToSD(exportColor), attrs.MinuteTimeWidth);
            using var hourPen = new Pen(attrs.TimeColor.ToSD(exportColor), attrs.HourTimeWidth);
            
            foreach (var l in GetTimeLines(out bool hour, startTime, endTime))
            {
                var offset = margin.Top + l * attrs.HeightPerHour / 60f;
                g.DrawLine(hour ? hourPen : minutePen, margin.Left - 5, offset, width - margin.Right, offset); // Linie

                var text = new TimeEntry(0, l + startTime.GetTotalMinutes()).Normalize().ToShortTimeString();
                var size = g.MeasureString(timeFont, text);
                g.DrawText(timeFont, timeBrush, margin.Left - 5 - size.Width, offset - (size.Height / 2), text); // Beschriftung
                hour = !hour;
            }
        }

        public float GetMarginLeftOffset(Graphics g, TimeEntry startTime, TimeEntry endTime)
        {
            var timeFont = (Font)attrs.TimeFont; // Reminder: Do not dispose, will be disposed with MFont instance!
            return GetTimeLines(out _, startTime, endTime)
                .Select(l => g.MeasureString((startTime + new TimeEntry(0, l)).Normalize().ToShortTimeString(), timeFont).Width)
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
}

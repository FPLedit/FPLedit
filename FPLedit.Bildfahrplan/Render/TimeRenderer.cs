using FPLedit.Bildfahrplan.Model;
using System;
using System.Collections.Generic;
using Eto.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPLedit.Bildfahrplan.Render
{
    internal class TimeRenderer
    {
        private TimetableStyle attrs;
        private Renderer parent;

        public TimeRenderer(TimetableStyle attrs, Renderer parent)
        {
            this.attrs = attrs;
            this.parent = parent;
        }

        public void Render(Graphics g, Margins margin, TimeSpan startTime, TimeSpan endTime, float width)
        {
            var timeBrush = new SolidBrush((Color)attrs.TimeColor);
            var minutePen = new Pen((Color)attrs.TimeColor, attrs.MinuteTimeWidth);
            var hourPen = new Pen((Color)attrs.TimeColor, attrs.HourTimeWidth);
            foreach (var l in parent.GetTimeLines(out bool hour, startTime, endTime))
            {
                var offset = margin.Top + l * attrs.HeightPerHour / 60f;
                g.DrawLine(hour ? hourPen : minutePen, margin.Left - 5, offset, width - margin.Right, offset); // Linie

                var text = new TimeSpan(0, l + startTime.GetMinutes(), 0).ToString(Renderer.TIME_FORMAT);
                var size = g.MeasureString((Font)attrs.TimeFont, text);
                g.DrawText((Font)attrs.TimeFont, timeBrush, margin.Left - 5 - size.Width, offset - (size.Height / 2), text); // Beschriftung
                hour = !hour;
            }
        }
    }
}

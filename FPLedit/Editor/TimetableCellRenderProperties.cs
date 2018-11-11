using Eto.Drawing;
using Eto.Forms;
using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPLedit.Editor
{
    internal class TimetableCellRenderProperties
    {
        private static Font fn, fb;
        private static Color errorColor;

        public string Text { get; set; }
        public Color Background { get; set; }
        public Font Font { get; set; }
        public bool ReadOnly { get; set; }

        public TimetableCellRenderProperties(Func<ArrDep, TimeSpan> time, Station sta, bool arrival, TimetableDataElement data)
        {
            if (fn == null || fb == null || errorColor == null)
            {
                fb = SystemFonts.Bold();
                fn = SystemFonts.Default();
                errorColor = new Color(Colors.Red, 0.4f);
            }

            Background = Colors.White;
            Font = fn;

            var ardp = data.ArrDeps[sta];

            if (!data.HasError(sta, arrival))
                Text = time(ardp) != default(TimeSpan) ? time(ardp).ToShortTimeString() : "";
            else
                Text = data.GetErrorText(sta, arrival);

            var first = data.IsFirst(sta);
            if ((!arrival && data.IsLast(sta)) || (arrival && first))
            {
                ReadOnly = true;
                Background = Colors.DarkGray;
            }
            else if (arrival ^ first)
            {
                if (first && ardp.TrapeztafelHalt)
                    throw new Exception("Die erste Station darf keinen Trapeztafelhalt beinhalten!");

                Background = ardp.TrapeztafelHalt ? Colors.LightGrey : Colors.White;
                if (ardp.Zuglaufmeldung != null && ardp.Zuglaufmeldung != "")
                    Font = fb;
            }

            if (data.HasError(sta, arrival))
                Background = errorColor;
        }

        public void Apply(TextBox tb)
        {
            tb.BackgroundColor = Background;
            tb.Font = Font;
            tb.ReadOnly = ReadOnly;
            tb.Text = Text ?? tb.Text;
        }

        public void Render(Graphics g, RectangleF clip)
        {
            g.Clear(Background);
            g.DrawText(Font, Colors.Black, new PointF(clip.Left + 2, clip.Top + 2), Text ?? "");
            g.DrawRectangle(Colors.Black, clip);
        }
    }
}

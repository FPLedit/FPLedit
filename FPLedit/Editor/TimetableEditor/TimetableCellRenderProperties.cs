using Eto.Drawing;
using Eto.Forms;
using FPLedit.Shared;
using System;

namespace FPLedit.Editor.TimetableEditor
{
    internal class TimetableCellRenderProperties
    {
        private static Font fn, fb;
        private static Color? errorColor, bgColor, textColor;
        
        public string Text { get; set; }
        public Color Background { get; set; }
        public Font Font { get; set; }
        public bool ReadOnly { get; set; }

        public TimetableCellRenderProperties(Func<ArrDep, TimeEntry> time, Station sta, bool arrival, BaseTimetableDataElement data)
        {
            if (fn == null || fb == null || errorColor == null || bgColor == null || textColor == null)
            {
                fb = SystemFonts.Bold();
                fn = SystemFonts.Default();
                var errorHsl = Colors.Red.ToHSL();
                errorHsl.L *= 0.8f;
                errorColor = errorHsl.ToColor();
                bgColor = SystemColors.ControlBackground;
                textColor = SystemColors.ControlText;
            }

            Background = bgColor.Value;
            Font = fn;

            var ardp = data.ArrDeps[sta];

            if (!data.HasError(sta, arrival))
                Text = time(ardp) != default ? time(ardp).ToShortTimeString() : "";
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

                Background = ardp.TrapeztafelHalt ? Colors.LightGrey : bgColor.Value;
                if (ardp.Zuglaufmeldung != null && ardp.Zuglaufmeldung != "")
                    Font = fb;
            }

            if (data.HasError(sta, arrival))
                Background = errorColor.Value;
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
            g.Clear(Colors.Transparent);
            g.FillRectangle(Background, clip);
            
            // Adjust text color, if contrast is too low.
            var tc = textColor.Value;
            var lt = 0.2126 * textColor.Value.R + 0.7152 * textColor.Value.G + 0.0722 * textColor.Value.B;
            var lb = 0.2126 * Background.R + 0.7152 * Background.G + 0.0722 * Background.B;
            if (lb - lb > 0 && lb - lt < 0.1)
                tc = Colors.White;
            if (lt - lb > 0 && lt - lb < 0.1)
                tc = Colors.Black;

            g.DrawText(Font, tc, new PointF(clip.Left + 2, clip.Top + 2), Text ?? "");
            g.DrawRectangle(textColor.Value, clip);
        }
    }
}

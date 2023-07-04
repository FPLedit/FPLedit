using Eto.Drawing;
using Eto.Forms;
using FPLedit.Shared;
using System;

namespace FPLedit.Editor.TimetableEditor
{
    internal sealed class TimetableCellRenderProperties
    {
        private static bool initialized;
        private static Font fn = null!, fb = null!, fc = null!, fbc = null!;
        private static Color errorColor, bgColor, textColor;

        private string? Text { get; }
        private Color Background { get; }
        private Font Font { get; }
        private bool ReadOnly { get; }

        public TimetableCellRenderProperties(Func<ArrDep, TimeEntry> time, Station sta, bool arrival, BaseTimetableDataElement data)
        {
            if (!initialized)
            {
                fb = SystemFonts.Bold();
                fn = SystemFonts.Default();
                fc = SystemFonts.Cached(SystemFont.Default, null, FontDecoration.Underline);
                fbc = SystemFonts.Cached(SystemFont.Bold, null, FontDecoration.Underline);
                var errorHsl = Colors.Red.ToHSL();
                errorHsl.L *= 0.8f;
                errorColor = errorHsl.ToColor();
                bgColor = SystemColors.ControlBackground;
                textColor = SystemColors.ControlText;
                initialized = true;
            }

            Background = bgColor;
            Font = fn;

            var ardp = data.ArrDeps![sta];

            if (!data.HasError(sta, arrival))
                Text = time(ardp) != default ? time(ardp).ToTimeString() : "";
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
                    throw new Exception(T._("Die erste Station darf keinen Trapeztafelhalt beinhalten!"));

                Background = ardp.TrapeztafelHalt ? Colors.LightGrey : bgColor;
                if (ardp.RequestStop)
                    Font = fc;
                if (!string.IsNullOrEmpty(ardp.Zuglaufmeldung) && !ardp.RequestStop)
                    Font = fb;
                if (!string.IsNullOrEmpty(ardp.Zuglaufmeldung) && ardp.RequestStop)
                    Font = fbc;
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
            tb.TextColor = AdjustTextContrastColor(textColor);
        }

        public void Render(Graphics g, RectangleF clip)
        {
            g.Clear(Colors.Transparent);
            g.FillRectangle(Background, clip);

            // Adjust text color, if contrast is too low.
            var tc = AdjustTextContrastColor(textColor);

            g.DrawText(Font, tc, new PointF(clip.Left + 2, clip.Top + 2), Text ?? "");
            g.DrawRectangle(textColor, clip);
        }

        private Color AdjustTextContrastColor(Color tc)
        {
            var lt = 0.2126 * tc.R + 0.7152 * tc.G + 0.0722 * tc.B;
            var lb = 0.2126 * Background.R + 0.7152 * Background.G + 0.0722 * Background.B;
            if (lb - lb > 0 && lb - lt < 0.1)
                tc = Colors.White;
            if (lt - lb > 0 && lt - lb < 0.1)
                tc = Colors.Black;
            return tc;
        }
    }

    internal sealed class TimetableCellRenderProperties2
    {
        private static bool initialized;
        private static Font fn = null!;
        private static Color bgColor, textColor;

        public string? Text { get; }
        public Color Background { get; }
        public Font Font { get; }

        public TimetableCellRenderProperties2(string text)
        {
            if (!initialized)
            {
                fn = SystemFonts.Default();
                bgColor = SystemColors.ControlBackground;
                textColor = SystemColors.ControlText;
                initialized = true;
            }

            Background = bgColor;
            Font = fn;
            Text = text;
        }

        public void Render(Graphics g, RectangleF clip)
        {
            g.Clear(Colors.Transparent);
            g.FillRectangle(Background, clip);

            // Adjust text color, if contrast is too low.
            var tc = textColor;
            var lt = 0.2126 * textColor.R + 0.7152 * textColor.G + 0.0722 * textColor.B;
            var lb = 0.2126 * Background.R + 0.7152 * Background.G + 0.0722 * Background.B;
            if (lb - lb > 0 && lb - lt < 0.1)
                tc = Colors.White;
            if (lt - lb > 0 && lt - lb < 0.1)
                tc = Colors.Black;

            g.DrawText(Font, tc, new PointF(clip.Left + 2, clip.Top + 2), Text ?? "");
            g.DrawRectangle(textColor, clip);
        }
    }
}
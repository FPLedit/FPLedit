using Eto.Drawing;
using System;

namespace FPLedit.Shared.Rendering
{
    public sealed class RenderBtn<T> : IDisposable
    {
        private readonly Font font = new Font(FontFamilies.SansFamilyName, 8);

        public T Tag { get; }

        public Point Location { get; set; }

        public Size Size { get; set; }

        public Rectangle Rect => new Rectangle(Location, Size);

        public Color BackgroundColor { get; set; }
        
        public Color? ForegroundColor { get; set; }

        public string Text { get; set; }

        public event EventHandler? Click;

        public event EventHandler? RightClick;

        public event EventHandler? DoubleClick;

        public RenderBtn(T data, Point loc, Size size, Color bg, string text = "", Color? fg = null)
        {
            Tag = data;
            Location = loc;
            Size = size;
            BackgroundColor = bg;
            ForegroundColor = fg;
            Text = text;
        }

        public void HandleClick(Point clickPosition, Point pan)
        {
            if (Rect.Contains(clickPosition - pan))
                Click?.Invoke(this, new EventArgs());
        }

        public void HandleRightClick(Point clickPosition, Point pan)
        {
            if (Rect.Contains(clickPosition - pan))
                RightClick?.Invoke(this, new EventArgs());
        }

        public void HandleDoubleClick(Point clickPosition, Point pan)
        {
            if (Rect.Contains(clickPosition - pan))
                DoubleClick?.Invoke(this, new EventArgs());
        }

        public void Draw(Graphics g)
        {
            g.FillRectangle(BackgroundColor, Rect);
            if (!string.IsNullOrEmpty(Text))
            {
                var size = g.MeasureString(font, Text);
                var color = ForegroundColor ?? Colors.Black;
                g.DrawText(font, color, Rect.MiddleX - (size.Width / 2), Rect.MiddleY - (size.Height / 2), Text);
            }
        }

        public void Dispose()
        {
            if (font != null && !font.IsDisposed)
                font.Dispose();
        }
    }
}

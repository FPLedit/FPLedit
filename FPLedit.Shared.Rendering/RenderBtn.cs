using Eto.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPLedit.Shared.Rendering
{
    public class RenderBtn<T>
    {
        private Font font = new Font(FontFamilies.SansFamilyName, 8);

        public T Tag { get; set; }

        public Point Location { get; set; }

        public Size Size { get; set; }

        public Rectangle Rect => new Rectangle(Location, Size);

        public Color Color { get; set; }

        public string Text { get; set; }

        public event EventHandler Click;

        public event EventHandler RightClick;

        public event EventHandler DoubleClick;

        public RenderBtn(T data, Point loc, Size size, Color c, string text = "")
        {
            Tag = data;
            Location = loc;
            Size = size;
            Color = c;
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
            g.FillRectangle(Color, Rect);
            if (Text != null && Text != "")
            {
                var size = g.MeasureString(font, Text);
                g.DrawText(font, Colors.Black, Rect.MiddleX - (size.Width / 2), Rect.MiddleY - (size.Height / 2), Text);
            }
        }
    }
}

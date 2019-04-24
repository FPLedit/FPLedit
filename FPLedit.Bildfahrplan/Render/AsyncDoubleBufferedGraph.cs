using Eto.Drawing;
using Eto.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPLedit.Bildfahrplan.Render
{
    internal class AsyncDoubleBufferedGraph
    {
        private Bitmap buffer;
        private bool generatingBuffer = false;
        private readonly Font font = new Font(FontFamilies.SansFamilyName, 8);
        private Panel panel;
        private object bufferLock = new object();
        public Action RenderingFinished { get; set; }

        public AsyncDoubleBufferedGraph(Panel p)
        {
            panel = p;
        }

        public void Render(Renderer renderer, Graphics g, bool drawHeader)
        {
            if (renderer == null)
                return;

            if (renderer.width != panel.Width)
            {
                renderer.width = panel.Width;
                Invalidate();
            }

            if (buffer == null && !generatingBuffer)
            {
                generatingBuffer = true;
                Task.Run(() =>
                {
                    var newBuffer = new Bitmap(panel.Width, renderer.GetHeight(drawHeader), PixelFormat.Format32bppRgb);
                    using (var g2 = new Graphics(newBuffer))
                        renderer.Draw(g2, drawHeader);
                    lock (bufferLock)
                    {
                        buffer = newBuffer;
                    }
                    generatingBuffer = false;
                    Application.Instance.Invoke(() =>
                    {
                        panel.Invalidate();
                        RenderingFinished?.Invoke();
                    });
                });
            }
            else if (buffer == null && generatingBuffer)
            {
                g.Clear(Colors.White);
                var text = "Generiere Bildfahplan...";
                var t = g.MeasureString(font, text);
                g.DrawText(font, Colors.Black, (panel.Width - t.Width) / 2, (panel.Height - t.Height) / 2, text);
            }
            else if (buffer != null)
                g.DrawImage(buffer, 0f, 0f);
        }

        public void Invalidate()
        {
            lock (bufferLock)
            {
                buffer?.Dispose();
                buffer = null;
                panel.Invalidate();
            }
        }
    }
}

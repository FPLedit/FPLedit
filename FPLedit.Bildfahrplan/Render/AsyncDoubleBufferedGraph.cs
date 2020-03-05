using Eto.Drawing;
using Eto.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPLedit.Bildfahrplan.Render
{
    internal class AsyncDoubleBufferedGraph : IDisposable
    {
        private Bitmap buffer;
        private bool generatingBuffer = false;
        private readonly Font font = new Font(FontFamilies.SansFamilyName, 8);
        private readonly Panel panel;
        private readonly object bufferLock = new object();
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
                Invalidate(true);

            if (buffer == null && !generatingBuffer)
            {
                generatingBuffer = true;
                Task.Run(() =>
                {
                    renderer.width = panel.Width;
                    var newBuffer = new Bitmap(panel.Width, renderer.GetHeight(drawHeader), PixelFormat.Format32bppRgb);
                    using (var ib = new ImageBridge(panel.Width, renderer.GetHeight(drawHeader)))
                    using (var etoGraphics = new Graphics(newBuffer))
                    {
                        renderer.Draw(ib.Graphics, drawHeader);
                        ib.CoptyToEto(etoGraphics);
                    }

                    lock (bufferLock)
                    {
                        if (buffer != null && !buffer.IsDisposed)
                            buffer.Dispose();
                        buffer = newBuffer;
                    }
                    generatingBuffer = false;
                    Application.Instance.Invoke(() =>
                    {
                        panel.Invalidate();
                        RenderingFinished?.Invoke();
                        GC.Collect();
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
            {
                lock (bufferLock)
                    g.DrawImage(buffer, 0f, 0f);
            }
        }

        public void Invalidate() => Invalidate(false);

        private void Invalidate(bool invalidatingControl)
        {
            lock (bufferLock)
            {
                if (buffer != null && !buffer.IsDisposed)
                    buffer.Dispose();
                buffer = null;
                if (!invalidatingControl)
                    panel.Invalidate();
                GC.Collect();
            }
        }

        public void Dispose()
        {
            if (buffer != null && !buffer.IsDisposed)
                buffer.Dispose();
            if (panel != null && !panel.IsDisposed)
                panel.Dispose();
            if (font != null && !font.IsDisposed)
                font.Dispose();
        }
    }
}
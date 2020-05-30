using Eto.Drawing;
using Eto.Forms;
using System;
using System.Threading.Tasks;
using FPLedit.Shared;
using FPLedit.Shared.Rendering;

namespace FPLedit.Bildfahrplan.Render
{
    internal class AsyncDoubleBufferedGraph : IDisposable
    {
        private Bitmap buffer;
        private bool generatingBuffer, hadCrash;
        private float lastBufferWidth;
        
        private readonly IPluginInterface pluginInterface;
        private readonly Font font = new Font(FontFamilies.SansFamilyName, 12);
        private readonly Panel panel;
        private readonly object bufferLock = new object();

        public Action RenderingFinished { get; set; }

        public AsyncDoubleBufferedGraph(Panel p, IPluginInterface pluginInterface)
        {
            this.pluginInterface = pluginInterface;
            panel = p;
        }

        public void Render(Renderer renderer, Graphics g, bool drawHeader)
        {
            if (renderer == null)
                return;

            if (Math.Abs(lastBufferWidth - panel.Width) > 0.01f)
                Invalidate(true);

            if (!hadCrash && buffer == null && !generatingBuffer)
            {
                generatingBuffer = true;
                Task.Run(() =>
                {
                    Bitmap newBuffer = null;
                    try
                    {
                        newBuffer = new Bitmap(panel.Width, renderer.GetHeightExternal(drawHeader), PixelFormat.Format32bppRgb);
                        using (var ib = new ImageBridge(panel.Width, renderer.GetHeightExternal(drawHeader)))
                        using (var etoGraphics = new Graphics(newBuffer))
                        {
                            renderer.Draw(ib.Graphics, drawHeader, forceWidth: panel.Width);
                            lastBufferWidth = panel.Width;
                            ib.CoptyToEto(etoGraphics);
                        }

                        lock (bufferLock)
                        {
                            if (buffer != null && !buffer.IsDisposed)
                                buffer.Dispose();
                            buffer = newBuffer;
                        }

                        generatingBuffer = false;
                    }
                    catch (Exception ex)
                    {
                        pluginInterface.Logger.LogException(ex);
                        lock (bufferLock)
                        {
                            if (buffer != null && !buffer.IsDisposed)
                                buffer.Dispose();
                        }

                        if (newBuffer != null && !newBuffer.IsDisposed)
                            newBuffer.Dispose();

                        generatingBuffer = false;
                        hadCrash = true;
                    }

                    Application.Instance.Invoke(() =>
                    {
                        panel.Invalidate();
                        RenderingFinished?.Invoke();
                        GC.Collect();
                    });
                });
            }
            else if (!hadCrash && buffer == null && generatingBuffer)
            {
                g.Clear(Colors.White);
                var text = "Generiere Bildfahrplan...";
                var t = g.MeasureString(font, text);
                g.DrawText(font, Colors.Black, (panel.Width - t.Width) / 2, 30, text);
            }
            else if (!hadCrash && buffer != null)
            {
                lock (bufferLock)
                    g.DrawImage(buffer, 0f, 0f);
            }
            else // We had a crash
            {
                g.Clear(Colors.White);
                var text = "Fehler beim Rendern (siehe Log)...";
                var t = g.MeasureString(font, text);
                g.DrawText(font, Colors.Red, (panel.Width - t.Width) / 2, 30, text);
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
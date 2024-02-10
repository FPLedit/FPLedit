using Eto.Drawing;
using Eto.Forms;
using System;
using System.Threading.Tasks;
using FPLedit.Shared;
using FPLedit.Shared.Rendering;

namespace FPLedit.Bildfahrplan.Render;

internal class AsyncDoubleBufferedGraph : IDisposable
{
    private Bitmap? buffer;
    private bool generatingBuffer, hadCrash, hadAmbiguousTransitions, hadInvalidVroute;
    private float lastBufferWidth;

    private readonly IPluginInterface pluginInterface;
    private readonly Font font = new (FontFamilies.SansFamilyName, 12);
    private readonly Panel panel;
    private readonly object bufferLock = new ();

    public Action? RenderingFinished { get; set; }

    public AsyncDoubleBufferedGraph(Panel p, IPluginInterface pluginInterface)
    {
        this.pluginInterface = pluginInterface;
        panel = p;
    }

    public void Render(Renderer? renderer, Graphics g, bool drawHeader)
    {
        if (renderer == null)
            return;

        if (Math.Abs(lastBufferWidth - panel.Width) > 0.01f)
            Invalidate(true);

        if (!hadCrash && buffer == null && !generatingBuffer)
        {
            generatingBuffer = true;
            var width = panel.Width;
            Task.Run(() =>
            {
                Bitmap? newBuffer = null;
                try
                {
                    var height = renderer.GetHeightExternal(drawHeader);
                    using var g2 = MGraphics.CreateImage(width, height);
                    g2.SetTextAntiAlias(true);
                    g2.Mutate(g3 => renderer.Draw(g3, drawHeader, forceWidth: panel.Width));

                    newBuffer = new Bitmap(width, height, PixelFormat.Format32bppRgba);
                    using (var etoGraphics = new Graphics(newBuffer))
                    using (var eto = g2.LockEtoBitmap())
                        etoGraphics.DrawImage(eto, 0, 0);
                    lastBufferWidth = width;

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
                    if (ex is not AmbiguousTransitionException && ex is not VirtualRouteInvalidEception)
                        pluginInterface.Logger.LogException(ex);
                    lock (bufferLock)
                    {
                        if (buffer != null && !buffer.IsDisposed)
                            buffer.Dispose();
                        buffer = null;
                    }

                    if (newBuffer != null && !newBuffer.IsDisposed)
                        newBuffer.Dispose();
                    newBuffer = null;

                    generatingBuffer = false;
                    hadCrash = true;
                    hadAmbiguousTransitions = ex is AmbiguousTransitionException;
                    hadInvalidVroute = ex is VirtualRouteInvalidEception;
                }

                Application.Instance.Invoke(() =>
                {
                    panel.Invalidate();
                    RenderingFinished?.Invoke();
                });
            });
        }
        else if (!hadCrash && buffer == null && generatingBuffer)
        {
            g.Clear(Colors.White);
            var text = T._("Generiere Bildfahrplan...");
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
            var text = T._("Fehler beim Rendern (siehe Log)...");
            if (hadAmbiguousTransitions)
                text = T._("Mehrere Folgezüge gefunden!\nBitte oben angezeigte Tage einschränken\nund Bildfahrplanvorschau erneut öffnen.");
            if (hadInvalidVroute)
                text = T._("Ungültige (zusammengefallene) virtuelle Strecke!\nBitte die virtuelle Strecke neu anlegen.");
            RenderError(g, text);
        }
    }

    public void RenderError(Graphics g, string text)
    {
        g.Clear(Colors.White);

        var ft = new FormattedText { Text = text, Font = font, Alignment = FormattedTextAlignment.Center, ForegroundBrush = Brushes.Red };
        var t = ft.Measure();
        g.DrawText(ft, new PointF((panel.Width - t.Width) / 2, 30));
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
        if (panel != null! && !panel.IsDisposed)
            panel.Dispose();
        if (font != null! && !font.IsDisposed)
            font.Dispose();
    }
}
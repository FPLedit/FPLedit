using System;
using ed = Eto.Drawing;

namespace FPLedit.Shared.Rendering
{
    public sealed class ImageBridge : IDisposable
    {
        public IMGraphics Graphics { get; }

        public ImageBridge(int width, int height)
        {
            Graphics = MGraphics.CreateImage(width, height);
            Graphics.SetTextAntiAlias(true);
        }

        public void CoptyToEto(ed.Graphics graphics)
        {
            using (var eto = Graphics.LockEtoBitmap())
                graphics.DrawImage(eto, 0, 0);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Graphics?.Dispose();
            GC.Collect();
        }

        ~ImageBridge()
        {
            Dispose();
        }
    }
}
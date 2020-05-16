using System;
using sd = System.Drawing;
using ed = Eto.Drawing;
using System.Runtime.CompilerServices;

namespace FPLedit.Bildfahrplan.Render
{
    internal sealed class ImageBridge : IDisposable
    {
        private readonly sd.Bitmap sdBuffer;
        private readonly sd.Graphics sdGraphics;

        public sd.Graphics Graphics => sdGraphics;

        public ImageBridge(int width, int height)
        {
            sdBuffer = new sd.Bitmap(width, height, sd.Imaging.PixelFormat.Format32bppRgb);
            sdGraphics = sd.Graphics.FromImage(sdBuffer);
            sdGraphics.TextRenderingHint = sd.Text.TextRenderingHint.AntiAlias;
        }

        public ImageBridge(ed.RectangleF rec) : this(width: (int)rec.Width, height: (int)rec.Height)
        {
        }

        private ed.Bitmap LockEtoBitmap()
        {
            sdGraphics?.Flush();
            
            var sdData = sdBuffer.LockBits(new sd.Rectangle(0, 0, sdBuffer.Width, sdBuffer.Height), sd.Imaging.ImageLockMode.ReadOnly, sdBuffer.PixelFormat);
            var byteLength = sdData.Height * sdData.Width * (((int) sdBuffer.PixelFormat >> 11) & 31);
            
            var etoBuffer = new ed.Bitmap(sdBuffer.Width, sdBuffer.Height, ed.PixelFormat.Format32bppRgb);
            var etoData = etoBuffer.Lock();

            unsafe
            {
                Buffer.MemoryCopy((void*) sdData.Scan0, (void*) etoData.Data, byteLength, byteLength);
            }
            
            etoData.Dispose();
            sdBuffer.UnlockBits(sdData);
            
            return etoBuffer;
        }

        public void CoptyToEto(ed.Graphics graphics)
        {
            using (var eto = LockEtoBitmap())
                graphics.DrawImage(eto, 0, 0);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            sdBuffer?.Dispose();
            sdGraphics?.Dispose();
            GC.Collect();
        }

        ~ImageBridge()
        {
            Dispose();
        }
    }

    /// <summary>
    /// Helper methods, aligning System.Drawing APIs a bit closer to Eto.Drawing to allow faster switching.
    /// </summary>
    internal static class GraphicsExt
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DrawText(this sd.Graphics graphics, sd.Font font, sd.Brush brush, float x, float y, string text)
            => graphics.DrawString(text, font, brush, x, y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static sd.SizeF MeasureString(this sd.Graphics graphics, sd.Font font, string text)
            => graphics.MeasureString(text, font);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MoveTo(this sd.Drawing2D.GraphicsPath path, sd.PointF point) => path.StartFigure();
    }
}
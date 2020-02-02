using System;
using System.Collections.Generic;
using sd = System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ed = Eto.Drawing;
using System.IO;
using System.Runtime.CompilerServices;

namespace FPLedit.Bildfahrplan.Render
{
    internal class ImageBridge : IDisposable
    {
        private readonly sd.Bitmap sdBuffer;
        private sd.Graphics sdGraphics;

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

        public ed.Bitmap LockEtoBitmap()
        {
            sdGraphics?.Flush();
            using (var ms = new MemoryStream())
            {
                sdBuffer.Save(ms, sd.Imaging.ImageFormat.Png);
                return new ed.Bitmap(ms.ToArray());
            }
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
using System;
using System.Threading.Tasks;
using sd = System.Drawing;
using ed = Eto.Drawing;

namespace FPLedit.Shared.Rendering
{
    public sealed class ImageBridge : IDisposable
    {
        private readonly sd.Bitmap sdBuffer;
        private readonly sd.Graphics sdGraphics;

        public sd.Graphics Graphics => sdGraphics;

        public ImageBridge(int width, int height)
        {
            sdBuffer = new sd.Bitmap(width, height, sd.Imaging.PixelFormat.Format32bppArgb);
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
            var bytesPerPixel = ((int) sdBuffer.PixelFormat >> 11) & 31;
            var byteLength = sdData.Height * sdData.Width * bytesPerPixel;
            
            var etoBuffer = new ed.Bitmap(sdBuffer.Width, sdBuffer.Height, ed.PixelFormat.Format32bppRgba);
            var etoData = etoBuffer.Lock();
            
            if (sdData.Stride < 0)
                throw new Exception("Negative stride value encountered!");

            unsafe
            {
                if (sdData.Stride > 0 && sdData.Stride == sdData.Width * bytesPerPixel)
                    Buffer.MemoryCopy((void*) sdData.Scan0, (void*) etoData.Data, byteLength, byteLength);
                else // Slightly slower route using the given stride width 
                {
                    var switchColors = MColor.ShouldSwitchColors;
                    var scan = (byte*) sdData.Scan0;
                    var bytesPerScanLine = sdData.Width * bytesPerPixel;
                    Parallel.For(0, sdData.Height, i =>
                    {
                        var lineStart = (i * sdData.Stride);
                        var line = scan + (i * sdData.Stride);
                        for (int j = 0; j < bytesPerScanLine; j += bytesPerPixel)
                        {
                            var b = line[j];
                            var g = line[j + 1];
                            var r = line[j + 2];
                            var a = line[j + 3];
                            var c = ed.Color.FromArgb(switchColors ? b : r, g, switchColors ? r : b, a);
                            etoData.SetPixel(j / bytesPerPixel, i, c);
                        }
                    });
                }
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
}
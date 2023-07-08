using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Threading.Tasks;
using ed = Eto.Drawing;

namespace FPLedit.Shared.Rendering;

#if ENABLE_SYSTEM_DRAWING
public sealed class MGraphicsSystemDrawing : IMGraphics
{
    private readonly Graphics g;
    private Bitmap? image;
    private readonly bool exportColor;
    private readonly Dictionary<int, Pen> penCache = new();
    private readonly Dictionary<int, SolidBrush> brushCache = new();
    private bool disposeGraphics;

    public MGraphicsSystemDrawing(Graphics g, bool exportColor)
    {
        this.exportColor = exportColor;
        this.g = g;
    }

    public (float Width, float Height) MeasureString(MFont font, string text)
    {
        var x = g.MeasureString(text, (Font) font);
        return (x.Width, x.Height);
    }

    public void DrawLine((MColor c, float w, float[] ds) pen, float x1, float y1, float x2, float y2)
    {
        var penCacheKey = pen.GetHashCode();
        if (!penCache.TryGetValue(penCacheKey, out var sdPen))
        {
            sdPen = new Pen(pen.c.ToSD(exportColor), pen.w) { DashPattern = pen.ds };
            penCache[penCacheKey] = sdPen;
        }
        g.DrawLine(sdPen, x1, y1, x2, y2);
    }

    public void DrawText(MFont font, MColor solidColor, float x, float y, string text)
    {
        var brushCacheKey = solidColor.GetHashCode();
        if (!brushCache.TryGetValue(brushCacheKey, out var brush))
        {
            brush = new SolidBrush(solidColor.ToSD(exportColor));
            brushCache[brushCacheKey] = brush;
        }
        g.DrawString(text, (Font)font, brush, x, y);
    }

    public void Clear(MColor color) => g.Clear(color.ToSD(exportColor));

    public object StoreTransform() => g.Transform.Clone();

    public void TranslateTransform(float tX, float tY) => g.TranslateTransform(tX, tY);

    public void RotateTransform(float angle) => g.RotateTransform(angle);

    public void RestoreTransform(object matrix) => g.Transform = (Matrix)matrix;

    public void SetAntiAlias(bool enable) => g.SmoothingMode = enable ? SmoothingMode.AntiAlias : SmoothingMode.Default;

    public void SetTextAntiAlias(bool enable) => g.TextRenderingHint = enable ? TextRenderingHint.AntiAlias : TextRenderingHint.SystemDefault;

    public (float Width, float Height) GetDrawingArea() => (g.ClipBounds.Width, g.ClipBounds.Height);

    public void DrawPath((MColor c, int w, float[] ds) pen, List<IPathCmd> graphicsPath)
    {
        var penCacheKey = pen.GetHashCode();
        if (!penCache.TryGetValue(penCacheKey, out var sdPen))
        {
            sdPen = new Pen(pen.c.ToSD(exportColor), pen.w) { DashPattern = pen.ds };
            penCache[penCacheKey] = sdPen;
        }

        using var p = new GraphicsPath();
        foreach (var cmd in graphicsPath)
        {
            switch (cmd)
            {
                case PathMoveCmd: p.StartFigure(); break;
                case PathLineCmd line: p.AddLine((PointF) line.Start, (PointF) line.End); break;
                case PathBezierCmd bezier: p.AddBezier((PointF) bezier.ControlPoint1, (PointF) bezier.Control1, (PointF) bezier.ControlPoint2, (PointF) bezier.Control2); break;
                default: throw new ArgumentException($"{nameof(graphicsPath)} contains unknown command of type {cmd.GetType().Name}");
            }
        }
        g.DrawPath(sdPen, p);
    }

    public static IMGraphics CreateImage(int width, int height, bool exportColor)
    {
        var image = new Bitmap(width, height, PixelFormat.Format32bppArgb);
        var g = Graphics.FromImage(image);
        var g2 = new MGraphicsSystemDrawing(g, exportColor);
        g2.disposeGraphics = true;
        g2.image = image;
        return g2;
    }

    public void Dispose()
    {
        foreach (var pen in penCache.Values)
            pen.Dispose();
        penCache.Clear();
        foreach (var brush in brushCache.Values)
            brush.Dispose();
        brushCache.Clear();

        if (disposeGraphics)
            g.Dispose();

        image?.Dispose();
    }

    public void SaveImagePng(Stream stream)
    {
        if (image == null)
            throw new Exception("Trying to save graphics content not backed by image!");
        image.Save(stream, ImageFormat.Png);
    }

    public void Flush() => g.Flush();
    
    public ed.Bitmap LockEtoBitmap()
    {
        if (image == null)
            throw new Exception("Trying to save graphics content not backed by image!");

        g.Flush();
        
        var sdData = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadOnly, image.PixelFormat);
        var bytesPerPixel = ((int) image.PixelFormat >> 11) & 31;
        var byteLength = sdData.Height * sdData.Width * bytesPerPixel;
        
        var etoBuffer = new ed.Bitmap(image.Width, image.Height, ed.PixelFormat.Format32bppRgba);
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
        image.UnlockBits(sdData);
        
        return etoBuffer;
    }
}
#endif
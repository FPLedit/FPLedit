using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace FPLedit.Shared.Rendering;

public sealed class Graphics2 : IDisposable, IGraphics2
{
    private readonly Graphics g;
    private Bitmap? image;
    private readonly bool exportColor;
    private readonly Dictionary<int, Pen> penCache = new();
    private readonly Dictionary<int, SolidBrush> brushCache = new();
    private bool disposeGraphics;

    public Graphics2(Graphics g, bool exportColor)
    {
        this.exportColor = exportColor;
        this.g = g;
    }

    public (float Width, float Height) MeasureString(MFont timeFont, string text)
    {
        var x = g.MeasureString(text, (Font) timeFont);
        return (x.Width, x.Height);
    }
    public (float Width, float Height) MeasureString(string text, MFont timeFont)
    {
        var x = g.MeasureString(text, (Font) timeFont);
        return (x.Width, x.Height);
    }

    public void DrawLine((MColor c, float w) pen, float x1, float y1, float x2, float y2)
    {
        var penCacheKey = pen.GetHashCode();
        if (!penCache.TryGetValue(penCacheKey, out var sdPen))
        {
            sdPen = new Pen(pen.c.ToSD(exportColor), pen.w);
            penCache[penCacheKey] = sdPen;
        }
        g.DrawLine(sdPen, x1, y1, x2, y2);
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
        g.DrawText((Font)font, brush, x, y, text);
    }

    public void Clear(MColor color)
    {
        g.Clear(color.ToSD(exportColor));
    }

    public object StoreTransform()
    {
        return g.Transform.Clone();
    }

    public void TranslateTransform(float tX, float tY)
    {
        g.TranslateTransform(tX, tY);
    }

    public void RotateTransform(float angle)
    {
        g.RotateTransform(angle);
    }

    public void RestoreTransform(object matrix)
    {
        g.Transform = (Matrix)matrix;
    }

    public void SetAntiAlias(bool enable)
    {
        g.SmoothingMode = enable ? SmoothingMode.AntiAlias : SmoothingMode.Default;
    }

    public (float Width, float Height) GetDrawingArea()
    {
        return (g.ClipBounds.Width, g.ClipBounds.Height);
    }

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
                case PathMoveCmd move: p.MoveTo((PointF) move.To); break;
                case PathLineCmd line: p.AddLine((PointF) line.Start, (PointF) line.End); break;
                case PathBezierCmd bezier: p.AddBezier((PointF) bezier.ControlPoint1, (PointF) bezier.Control1, (PointF) bezier.ControlPoint2, (PointF) bezier.Control2); break;
                default: throw new ArgumentException($"{nameof(graphicsPath)} contains unknown command of type {cmd.GetType().Name}");
            }
        }
        g.DrawPath(sdPen, p);
    }

    public static Graphics2 CreateImage(int width, int height)
    {
        var image = new Bitmap(width, height, PixelFormat.Format24bppRgb);
        var g = Graphics.FromImage(image);
        var g2 = new Graphics2(g, false);
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
        throw new NotImplementedException();
    }
}
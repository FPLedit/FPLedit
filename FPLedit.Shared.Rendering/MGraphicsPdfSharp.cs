using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PdfSharp.Drawing;
using ed = Eto.Drawing;

namespace FPLedit.Shared.Rendering;

public sealed class MGraphicsPdfSharp : IMGraphics
{
    private readonly XGraphics g;
    private readonly Dictionary<int, XPen> penCache = new();
    private readonly Dictionary<int, XSolidBrush> brushCache = new();

    public MGraphicsPdfSharp(XGraphics g)
    {
        this.g = g;
    }

    public void Mutate(Action<IMGraphics> action) => action(this);

    public (float Width, float Height) MeasureString(MFont font, string text)
    {
        var x = g.MeasureString(text, (XFont) font);
        return ((float)x.Width, (float)x.Height);
    }

    public void DrawLine((MColor c, float w, float[] ds) pen, float x1, float y1, float x2, float y2)
    {
        var penCacheKey = pen.GetHashCode();
        if (!penCache.TryGetValue(penCacheKey, out var sdPen))
        {
            sdPen = new XPen((XColor) pen.c, pen.w) { DashPattern = pen.ds.Select(f => (double) f).ToArray() };
            penCache[penCacheKey] = sdPen;
        }
        g.DrawLine(sdPen, x1, y1, x2, y2);
    }

    public void DrawText(MFont font, MColor solidColor, float x, float y, string text)
    {
        var brushCacheKey = solidColor.GetHashCode();
        if (!brushCache.TryGetValue(brushCacheKey, out var brush))
        {
            brush = new XSolidBrush((XColor) solidColor);
            brushCache[brushCacheKey] = brush;
        }
        g.DrawString(text, (XFont)font, brush, x, y, new XStringFormat { LineAlignment = XLineAlignment.Near });
    }

    public void Clear(MColor color) { }

    public object StoreTransform() => g.Save();

    public void TranslateTransform(float tX, float tY) => g.TranslateTransform(tX, tY);

    public void RotateTransform(float angle) => g.RotateTransform(angle);

    public void RestoreTransform(object matrix) => g.Restore((XGraphicsState) matrix);

    public void SetAntiAlias(bool enable) => g.SmoothingMode = enable ? XSmoothingMode.AntiAlias : XSmoothingMode.Default;

    public void SetTextAntiAlias(bool enable) => SetAntiAlias(enable);

    public (float Width, float Height) GetDrawingArea() => ((float) g.PageSize.Width, (float) g.PageSize.Height);

    public void DrawPath((MColor c, int w, float[] ds) pen, List<IPathCmd> graphicsPath)
    {
        var penCacheKey = pen.GetHashCode();
        if (!penCache.TryGetValue(penCacheKey, out var sdPen))
        {
            sdPen = new XPen((XColor) pen.c, pen.w) { DashPattern = pen.ds.Select(f => (double) f).ToArray() };
            penCache[penCacheKey] = sdPen;
        }

        var p = new XGraphicsPath();
        foreach (var cmd in graphicsPath)
        {
            switch (cmd)
            {
                case PathMoveCmd: p.StartFigure(); break;
                case PathLineCmd line: p.AddLine((XPoint) line.Start, (XPoint) line.End); break;
                case PathBezierCmd bezier: p.AddBezier((XPoint) bezier.ControlPoint1, (XPoint) bezier.Control1, (XPoint) bezier.ControlPoint2, (XPoint) bezier.Control2); break;
                default: throw new ArgumentException($"{nameof(graphicsPath)} contains unknown command of type {cmd.GetType().Name}");
            }
        }
        g.DrawPath(sdPen, p);
    }

    public void Dispose() { }

    public void SaveImagePng(Stream stream) => throw new NotSupportedException(nameof(MGraphicsPdfSharp) + " vcnnot be used for on-screen or PNG rendering!");

    public void Flush() { }

    public ed.Bitmap LockEtoBitmap() => throw new NotSupportedException(nameof(MGraphicsPdfSharp) + " vcnnot be used for on-screen or PNG rendering!");
}
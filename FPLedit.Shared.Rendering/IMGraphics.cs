using System;
using System.Collections.Generic;
using System.IO;

namespace FPLedit.Shared.Rendering;

/// <summary>
/// Abstraction for simple drawing on a 2D surface.
/// </summary>
/// <remarks>
/// This roughly resembles the API surface of <code>System.Drawing</code> or <code>Eto.Drawing</code>.
/// But it is not guaranteed, that one of those backends is actually used for drawing with this instance.
/// </remarks>
public interface IMGraphics : IDisposable
{
    (float Width, float Height) MeasureString(MFont timeFont, string text);
    void DrawLine((MColor c, float w, float[] ds) pen, float x1, float y1, float x2, float y2);
    void DrawText(MFont font, MColor solidColor, float x, float y, string text);
    void Clear(MColor color);
    object StoreTransform();
    void TranslateTransform(float tX, float tY);
    void RotateTransform(float angle);
    void RestoreTransform(object matrix);
    void SetAntiAlias(bool enable);
    void SetTextAntiAlias(bool enable);
    (float Width, float Height) GetDrawingArea();
    void DrawPath((MColor c, int w, float[] ds) pen, List<IPathCmd> graphicsPath);
    void Flush();
    void SaveImagePng(Stream stream);
    Eto.Drawing.Bitmap LockEtoBitmap();
}
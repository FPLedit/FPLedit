using System;
using System.Collections.Generic;
using System.IO;

namespace FPLedit.Shared.Rendering;

public interface IMGraphics : IDisposable
{
    (float Width, float Height) MeasureString(MFont timeFont, string text);
    (float Width, float Height) MeasureString(string text, MFont timeFont);
    void DrawLine((MColor c, float w) pen, float x1, float y1, float x2, float y2);
    void DrawLine((MColor c, float w, float[] ds) pen, float x1, float y1, float x2, float y2);
    void DrawText(MFont font, MColor solidColor, float x, float y, string text);
    void Clear(MColor color);
    object StoreTransform();
    void TranslateTransform(float tX, float tY);
    void RotateTransform(float angle);
    void RestoreTransform(object matrix);
    void SetAntiAlias(bool enable);
    (float Width, float Height) GetDrawingArea();
    void DrawPath((MColor c, int w, float[] ds) pen, List<IPathCmd> graphicsPath);
    void Flush();
    void SaveImagePng(Stream stream);
}
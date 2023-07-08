using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using ed = Eto.Drawing;

namespace FPLedit.Shared.Rendering;

public sealed class MGraphicsImageSharp : IMGraphics
{
    private readonly Image<Rgba32> image;
    private readonly Dictionary<int, Pen> penCache = new();

    // Internal state
    private Matrix3x2 matrix = Matrix3x2.Identity;
    private bool antiAlias = false;
    private bool textAntiAlias = false;

    private MGraphicsImageSharp(Image<Rgba32> image)
    {
        this.image = image;
    }

    private DrawingOptions GetDrawingOptions()
    {
        return new DrawingOptions
        {
            Transform = matrix,
            GraphicsOptions = new GraphicsOptions { Antialias = antiAlias || textAntiAlias },
        };
    }

    public (float Width, float Height) MeasureString(MFont font, string text)
    {
        var x = TextMeasurer.Measure(text, new TextOptions((Font) font));
        return (x.Width, x.Height);
    }

    public void DrawLine((MColor c, float w, float[] ds) pen, float x1, float y1, float x2, float y2)
    {
        var penCacheKey = pen.GetHashCode();
        if (!penCache.TryGetValue(penCacheKey, out var sdPen))
        {
            sdPen = new Pen((Color)pen.c, pen.w + 1, pen.ds); //TODO: I don't know why this `+1` is needed!?
            penCache[penCacheKey] = sdPen;
        }
        image.Mutate(ctx => ctx.DrawLines(GetDrawingOptions(), sdPen, new []{ new PointF(x1, y1), new PointF(x2, y2)}));
    }

    public void DrawText(MFont font, MColor solidColor, float x, float y, string text)
    {
        image.Mutate(ctx => ctx.DrawText(GetDrawingOptions(), text, (Font) font, (Color) solidColor, new PointF(x, y)));
    }

    public void Clear(MColor color) => image.Mutate(ctx => ctx.Clear((Color) color));

    public object StoreTransform() => matrix;

    public void TranslateTransform(float tX, float tY) => matrix *= Matrix3x2.CreateTranslation(tX, tY);

    public void RotateTransform(float angle) => matrix *= Matrix3x2.CreateRotation((float)(Math.PI * angle/180f), matrix.Translation);

    public void RestoreTransform(object m) => matrix = (Matrix3x2)m;

    public void SetAntiAlias(bool enable) => antiAlias = enable;

    public void SetTextAntiAlias(bool enable) => textAntiAlias = enable;

    public (float Width, float Height) GetDrawingArea() => (image.Width, image.Height);

    public void DrawPath((MColor c, int w, float[] ds) pen, List<IPathCmd> graphicsPath)
    {
        var penCacheKey = pen.GetHashCode();
        if (!penCache.TryGetValue(penCacheKey, out var isPen))
        {
            isPen = new Pen((Color) pen.c, pen.w, pen.ds);
            penCache[penCacheKey] = isPen;
        }
        
        image.Mutate(ctx =>
        {
            var dopt = GetDrawingOptions();
            foreach (var cmd in graphicsPath)
            {
                switch (cmd)
                {
                    case PathMoveCmd: break;
                    case PathLineCmd line: ctx.DrawLines(dopt, isPen, new[] { (PointF) line.Start, (PointF) line.End }); break;
                    case PathBezierCmd bezier: ctx.DrawBeziers(dopt, isPen, new[] {(PointF) bezier.ControlPoint1, (PointF) bezier.Control1, (PointF) bezier.ControlPoint2, (PointF) bezier.Control2} ); break;
                    default: throw new ArgumentException($"{nameof(graphicsPath)} contains unknown command of type {cmd.GetType().Name}");
                }
            }
        });
    }

    public static IMGraphics CreateImage(int width, int height)
    {
        var config = Configuration.Default.Clone();
        config.PreferContiguousImageBuffers = true; // Allow for copying to eto buffers below.
        var image = new Image<Rgba32>(config, width, height);
        return new MGraphicsImageSharp(image);
    }

    public void Dispose()
    {
        image.Dispose();
    }

    public void SaveImagePng(Stream stream)
    {
        if (image == null)
            throw new Exception("Trying to save graphics content not backed by image!");
        image.SaveAsPng(stream);
    }

    public void Flush() {}
    
    public ed.Bitmap LockEtoBitmap()
    {
        if (image == null)
            throw new Exception("Trying to save graphics content not backed by image!");

        if (!image.DangerousTryGetSinglePixelMemory(out Memory<Rgba32> memory))
            throw new Exception("getting single pixel memory failed!");

        var etoBuffer = new ed.Bitmap(image.Width, image.Height, ed.PixelFormat.Format32bppRgba);
        var etoData = etoBuffer.Lock();

        var byteLength = image.Height * image.Width * Unsafe.SizeOf<Rgba32>();
        if (memory.Length * Unsafe.SizeOf<Rgba32>() != byteLength || etoData.ScanWidth * etoBuffer.Height != byteLength)
            throw new Exception("Some weird stuff going on while copying memory"); 

        using (MemoryHandle pinHandle = memory.Pin())
        {
            unsafe
            {
                Buffer.MemoryCopy(pinHandle.Pointer, (void*) etoData.Data, byteLength, byteLength);
            }
        }

        etoData.Dispose();

        return etoBuffer;
    }
}
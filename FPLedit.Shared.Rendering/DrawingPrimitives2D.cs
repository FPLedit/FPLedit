namespace FPLedit.Shared.Rendering;

/// <summary>
/// A simple vector class for graphics abstraction, used for points and sizes.
/// </summary>
public readonly record struct Vec2(float X, float Y)
{
    public static Vec2 operator *(Vec2 v, float f) => new (v.X * f, v.Y * f);
    public static Vec2 operator *(int i, Vec2 v) => new (v.X * i, v.Y * i);
    public static Vec2 operator /(Vec2 v, float f) => v * (1 / f);
    public static Vec2 operator +(Vec2 v1, Vec2 v2) => new (v1.X + v2.X, v1.Y + v2.Y);
    public static Vec2 operator -(Vec2 v1, Vec2 v2) => new (v1.X - v2.X, v1.Y - v2.Y);
    public Vec2 Transpose => new (X, -Y);

#if ENABLE_SYSTEM_DRAWING
    public static explicit operator System.Drawing.PointF(Vec2 v) => new(v.X, v.Y);
#endif
    public static explicit operator SixLabors.ImageSharp.PointF(Vec2 v) => new(v.X, v.Y);
    public static explicit operator PdfSharp.Drawing.XPoint(Vec2 v) => new(v.X, v.Y);
    public override string ToString() => $"({X} | {Y})";
}

/// <summary>
/// Base interface for path actions that can be used in <see cref="IMGraphics.DrawPath"/>.
/// </summary>
public interface IPathCmd {}
/// <summary>
/// Break the current path, i.e. move without creating a line.
/// </summary>
/// <seealso cref="IMGraphics.DrawPath"/>
public record struct PathMoveCmd(Vec2 To) : IPathCmd;
/// <summary>
/// Draw a single line in this path.
/// </summary>
/// <seealso cref="IMGraphics.DrawPath"/>
public record struct PathLineCmd(Vec2 Start, Vec2 End): IPathCmd;
/// <summary>
/// Draw a single bezier curve in this path.
/// </summary>
/// <seealso cref="IMGraphics.DrawPath"/>
public record struct PathBezierCmd(Vec2 ControlPoint1, Vec2 Control1, Vec2 ControlPoint2, Vec2 Control2) : IPathCmd;
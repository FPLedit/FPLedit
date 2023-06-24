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
    public Vec2 Transpose => new(X, -Y);

#if ENABLE_SYSTEM_DRAWING
    public static explicit operator System.Drawing.PointF(Vec2 v) => new(v.X, v.Y);
#endif
    public static explicit operator SixLabors.ImageSharp.PointF(Vec2 v) => new(v.X, v.Y);
}

public interface IPathCmd {}
public record struct PathMoveCmd(Vec2 To) : IPathCmd;
public record struct PathLineCmd(Vec2 Start, Vec2 End): IPathCmd;
public record struct PathBezierCmd(Vec2 ControlPoint1, Vec2 Control1, Vec2 ControlPoint2, Vec2 Control2) : IPathCmd;
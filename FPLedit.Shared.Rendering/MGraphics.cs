namespace FPLedit.Shared.Rendering;

public static class MGraphics
{
    public static IMGraphics CreateImage(int width, int height, bool exportColor)
    {
        return MGraphicsSystemDrawing.CreateImage(width, height, exportColor);
    }
}
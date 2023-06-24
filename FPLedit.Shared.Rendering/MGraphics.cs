namespace FPLedit.Shared.Rendering;

public class MGraphics
{
    public static IMGraphics CreateImage(int width, int height)
    {
        return MGraphicsSystemDrawing.CreateImage(width, height);
    }
}
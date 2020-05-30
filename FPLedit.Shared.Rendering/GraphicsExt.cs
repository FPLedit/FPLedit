using System.Runtime.CompilerServices;
using sd = System.Drawing;
using ed = Eto.Drawing;


namespace FPLedit.Shared.Rendering
{

    /// <summary>
    /// Helper methods, aligning System.Drawing APIs a bit closer to Eto.Drawing to allow faster switching.
    /// </summary>
    public static class GraphicsExt
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DrawText(this sd.Graphics graphics, sd.Font font, sd.Brush brush, float x, float y, string text)
            => graphics.DrawString(text, font, brush, x, y);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static sd.SizeF MeasureString(this sd.Graphics graphics, sd.Font font, string text)
            => graphics.MeasureString(text, font);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void MoveTo(this sd.Drawing2D.GraphicsPath path, sd.PointF point) => path.StartFigure();
    }
}
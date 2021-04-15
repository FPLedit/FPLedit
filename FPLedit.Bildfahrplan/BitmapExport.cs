using FPLedit.Bildfahrplan.Render;
using FPLedit.Shared;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

namespace FPLedit.Bildfahrplan
{
    internal sealed class BitmapExport : IExport
    {
        public bool Export(Timetable tt, Stream stream, IReducedPluginInterface pluginInterface, string[] flags = null) 
        {
            Renderer renderer = new Renderer(tt, Renderer.DefaultPathData(Timetable.LINEAR_ROUTE_ID, tt));
            using (var bmp = new Bitmap(1000, renderer.GetHeightExternal(true), PixelFormat.Format32bppArgb))
            using (var g = Graphics.FromImage(bmp))
            {
                renderer.Draw(g, true);
                bmp.Save(stream, ImageFormat.Png);
            }
            return true;
        }

        public string Filter => T._("Bildfahrplan als PNG (*.png)|*.png");
    }
}

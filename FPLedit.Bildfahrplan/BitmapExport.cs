using FPLedit.Bildfahrplan.Render;
using FPLedit.Shared;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

namespace FPLedit.Bildfahrplan
{
    public class BitmapExport : IExport
    {
        public bool Export(Timetable tt, Stream stream, IPluginInterface pluginInterface, string[] flags = null) 
        {
            try
            {
                Renderer renderer = new Renderer(tt, Timetable.LINEAR_ROUTE_ID);
                using (var bmp = new Bitmap(1000, renderer.GetHeight(true), PixelFormat.Format32bppArgb))
                using (var g = Graphics.FromImage(bmp))
                {
                    renderer.Draw(g, true);
                    bmp.Save(stream, ImageFormat.Png);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public string Filter => "Bildfahrplan als PNG (*.png)|*.png";
    }
}

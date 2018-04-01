using FPLedit.BildfahrplanExport.Render;
using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPLedit.BildfahrplanExport
{
    public class BitmapExport : IExport
    {
        public string Filter => "Bildfahrplan als PNG (*.png)|*.png";

        public bool Export(Timetable tt, string filename, IInfo info)
        {
            try
            {
                Renderer renderer = new Renderer(tt, Timetable.LINEAR_ROUTE_ID);
                Bitmap bmp = new Bitmap(1000, renderer.GetHeight());
                using (var g = Graphics.FromImage(bmp))
                    renderer.Draw(g);
                bmp.Save(filename, ImageFormat.Png);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}

using FPLedit.Bildfahrplan.Render;
using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using Eto.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPLedit.Bildfahrplan
{
    public class BitmapExport : IExport
    {
        public bool Export(Timetable tt, Stream stream, IPluginInterface pluginInterface, string[] flags = null) 
        {
            try
            {
                Renderer renderer = new Renderer(tt, Timetable.LINEAR_ROUTE_ID);
                using (var bmp = new Bitmap(1000, renderer.GetHeight(true), PixelFormat.Format32bppRgba))
                using (var g = new Graphics(bmp))
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

        public bool Export(Timetable tt, string filename, IPluginInterface pluginInterface, string[] flags = null)
        {
            using (var stream = File.Open(filename, FileMode.OpenOrCreate, FileAccess.Write))
                return Export(tt, stream, pluginInterface, flags);
        }
    }
}

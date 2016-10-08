using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPLedit.BildfahrplanExport
{
    public class BitmapExport : IExport
    {
        public string Filter
        {
            get
            {
                return "Bildfahrplan als Bitmap (*.bmp)|*.bmp";
            }
        }

        public bool Reoppenable
        {
            get
            {
                return false;
            }
        }

        public bool Export(Timetable tt, string filename, ILog logger)
        {
            try
            {
                Renderer renderer = new Renderer(tt);
                Bitmap bmp = new Bitmap(1000, renderer.GetHeight());
                using (var g = Graphics.FromImage(bmp))
                    renderer.Draw(g);
                bmp.Save(filename);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}

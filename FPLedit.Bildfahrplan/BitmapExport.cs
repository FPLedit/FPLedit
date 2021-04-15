using System;
using FPLedit.Bildfahrplan.Render;
using FPLedit.Shared;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

namespace FPLedit.Bildfahrplan
{
    internal sealed class BitmapExport : IExport
    {
        private readonly int route;
        private readonly int width;
        private readonly ImageFormat format;

        public BitmapExport(int route, int width, ImageFormat format)
        {
            this.route = route;
            this.width = width;
            this.format = format;
        }

        public bool Export(Timetable tt, Stream stream, IReducedPluginInterface pluginInterface, string[] flags = null) 
        {
            Func<PathData> pd;
            if (route > Timetable.UNASSIGNED_ROUTE_ID)
                pd = Renderer.DefaultPathData(route, tt);
            else
            {
                var virt = VirtualRoute.GetVRoute(tt, route);
                pd = virt!.GetPathData;
            }
            
            Renderer renderer = new Renderer(tt, pd);
            using (var bmp = new Bitmap(width, renderer.GetHeightExternal(true), PixelFormat.Format32bppArgb))
            using (var g = Graphics.FromImage(bmp))
            {
                renderer.Draw(g, true, width);
                bmp.Save(stream, format);
            }
            return true;
        }

        public string Filter => T._("Bildfahrplan als PNG (*.png)|*.png");
    }
}

using System;
using FPLedit.Bildfahrplan.Render;
using FPLedit.Shared;
using System.IO;
using FPLedit.Shared.Rendering;

namespace FPLedit.Bildfahrplan
{
    internal sealed class BitmapExport : IExport
    {
        private readonly int route;
        private readonly int width;

        public BitmapExport(int route, int width)
        {
            this.route = route;
            this.width = width;
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
            using (var g2 = Graphics2.CreateImage(width, renderer.GetHeightExternal(true)))
            {
                renderer.Draw(g2, true, width);
                g2.SaveImagePng(stream);
            }
            return true;
        }

        public string Filter => T._("Bildfahrplan als PNG (*.png)|*.png");
    }
}

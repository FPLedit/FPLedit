using FPLedit.Bildfahrplan.Render;
using FPLedit.Shared;
using System.IO;
using FPLedit.Shared.Rendering;

namespace FPLedit.Bildfahrplan;

internal sealed class BitmapExport : IExport
{
    private readonly int route;
    private readonly int width;

    public BitmapExport(int route, int width)
    {
        this.route = route;
        this.width = width;
    }

    public bool Export(Timetable tt, Stream stream, IReducedPluginInterface pluginInterface, string[]? flags = null)
    {
        var pd = VirtualRoute.GetPathDataMaybeVirtual(tt, route);
        var renderer = new Renderer(tt, pd);

        using var g2 = MGraphics.CreateImage(width, renderer.GetHeightExternal(true));
        renderer.Draw(g2, true, width);
        g2.SaveImagePng(stream);
        return true;
    }

    public string Filter => T._("Bildfahrplan als PNG (*.png)|*.png");
}
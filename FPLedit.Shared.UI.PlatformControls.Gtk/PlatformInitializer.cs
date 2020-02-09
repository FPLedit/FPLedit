using Eto;
using FPLedit.Shared.UI.PlatformControls;

[assembly: ExportInitializer(typeof(FPLedit.Shared.UI.Gtk.PlatformInitializer))]

namespace FPLedit.Shared.UI.Gtk
{
    public sealed class PlatformInitializer : IPlatformInitializer
    {
        public void Initialize(Platform platform)
        {
            platform.Add<BDComboBoxCell.IHandler>(() => new BDComboBoxCellHandler());
        }
    }
}
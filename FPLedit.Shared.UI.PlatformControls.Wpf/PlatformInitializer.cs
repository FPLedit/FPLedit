using Eto;
using FPLedit.Shared.UI.PlatformControls;

[assembly: ExportInitializer(typeof(FPLedit.Shared.UI.Wpf.PlatformInitializer))]

namespace FPLedit.Shared.UI.Wpf
{
    public sealed class PlatformInitializer : IPlatformInitializer
    {
        public void Initialize(Platform platform)
        {
            platform.Add<BDComboBoxCell.IHandler>(() => new BDComboBoxCellHandler());
        }
    }
}

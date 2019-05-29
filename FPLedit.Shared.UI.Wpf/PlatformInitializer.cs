using Eto;
using FPLedit.Shared.UI.PlatformDependant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[assembly: ExportInitializer(typeof(FPLedit.Shared.UI.Wpf.PlatformInitializer))]

namespace FPLedit.Shared.UI.Wpf
{
    public class PlatformInitializer : IPlatformInitializer
    {
        public void Initialize(Platform platform)
        {
            platform.Add<BDComboBoxCell.IHandler>(() => new BDComboBoxCellHandler());
        }
    }
}

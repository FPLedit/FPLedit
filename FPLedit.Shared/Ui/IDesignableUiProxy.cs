using Eto.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FPLedit.Shared.Ui
{
    public interface IDesignableUiProxy
    {
        string DisplayName { get; }

        Control GetControl(IPluginInterface pluginInterface);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FPLedit.Shared.Ui
{
    public interface IEditingDialog
    {
        string DisplayName { get; }

        void Show(IPluginInterface pluginInterface);

        bool IsEnabled(IPluginInterface pluginInterface);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FPLedit.Shared.Ui
{
    public interface IPreviewable
    {
        string DisplayName { get; }

        void Show(IPluginInterface pluginInterface);
    }
}

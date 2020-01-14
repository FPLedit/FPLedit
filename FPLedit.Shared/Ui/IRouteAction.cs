using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FPLedit.Shared.Ui
{
    public interface IRouteAction
    {
        string DisplayName { get; }

        void Show(IPluginInterface pluginInterface, Route route);

        bool IsEnabled(IPluginInterface pluginInterface);
    }
}

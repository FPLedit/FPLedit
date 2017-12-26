using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FPLedit.Shared.Ui
{
    public interface IRouteAction
    {
        string DisplayName { get; }

        void Show(IInfo info, Route route);

        bool IsEnabled(IInfo info);
    }
}

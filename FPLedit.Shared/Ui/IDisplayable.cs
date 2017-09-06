using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FPLedit.Shared.Ui
{
    public interface IDisplayable
    {
        string DisplayName { get; }

        void Show(IInfo info);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FPLedit.Shared.Ui
{
    public interface IEditingDialog
    {
        string DisplayName { get; }

        object GroupObject { get; }

        void Show(IInfo info);

        bool IsEnabled(IInfo info);
    }
}

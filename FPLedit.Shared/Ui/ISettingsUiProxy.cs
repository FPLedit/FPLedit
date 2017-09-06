using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace FPLedit.Shared.Ui
{
    public interface ISettingsUiProxy
    {
        string DisplayName { get; }

        Control GetControl(IInfo info);
    }
}

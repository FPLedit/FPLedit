using FPLedit.Shared;
using FPLedit.Shared.Ui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace FPLedit.Buchfahrplan
{
    public class SettingsControlProxy : IDesignableUiProxy
    {
        public string DisplayName => "Buchfahrplan";

        public Control GetControl(IInfo info)
        {
            return new SettingsControl(info.Timetable, info);
        }
    }
}

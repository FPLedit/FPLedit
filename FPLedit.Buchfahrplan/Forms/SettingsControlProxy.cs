using Eto.Forms;
using FPLedit.Buchfahrplan.Forms;
using FPLedit.Shared;
using FPLedit.Shared.Ui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FPLedit.Buchfahrplan
{
    public class SettingsControlProxy : IDesignableUiProxy
    {
        public string DisplayName => "Buchfahrplan";

        public Control GetControl(IInfo info)
            => new SettingsControl(info.Timetable, info);
    }
}

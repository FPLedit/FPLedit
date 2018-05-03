using Eto.Forms;
using FPLedit.Shared;
using FPLedit.Shared.Ui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FPLedit.Kursbuch.Forms
{
    public class SettingsControlProxy : IDesignableUiProxy
    {
        public string DisplayName => "Kursbuch";

        public Control GetControl(IInfo info)
        {
            return new SettingsControl(info.Timetable, info);
        }
    }
}

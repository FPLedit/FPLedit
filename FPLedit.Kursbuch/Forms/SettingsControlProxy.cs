using Eto.Forms;
using FPLedit.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FPLedit.Kursbuch.Forms
{
    public class SettingsControlProxy : IAppearanceControl
    {
        public string DisplayName => "Kursbuch";

        public Control GetControl(IPluginInterface pluginInterface) 
            => new SettingsControl(pluginInterface.Timetable, pluginInterface);
    }
}

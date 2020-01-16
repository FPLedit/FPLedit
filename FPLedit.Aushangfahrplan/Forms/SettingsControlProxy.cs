using Eto.Forms;
using FPLedit.Shared;

namespace FPLedit.Aushangfahrplan.Forms
{
    public class SettingsControlProxy : IAppearanceControl
    {
        public string DisplayName => "Aushangfahrplan";

        public Control GetControl(IPluginInterface pluginInterface)
            => new SettingsControl(pluginInterface.Timetable, pluginInterface);
    }
}

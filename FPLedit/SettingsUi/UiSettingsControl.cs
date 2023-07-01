#nullable enable
using Eto.Drawing;
using Eto.Forms;
using FPLedit.Editor.Rendering;
using FPLedit.Shared;
using FPLedit.Shared.UI;

namespace FPLedit.SettingsUi
{
    public class UiSettingsControl : ISettingsControl
    {
        public string DisplayName => T._("Benutzeroberfläche");

        public Control GetControl(IPluginInterface pluginInterface)
        {
            var checkButton = new Button { Text = T._("Gespeicherte Fenstergrößen löschen") };
            var toolbarCheckBox = new CheckBox { Text = T._("Menüleiste mit Symbolen statt Text darstellen") };

            var stack = new StackLayout(toolbarCheckBox,  checkButton)
            {
                Padding = new Padding(10),
                Orientation = Orientation.Vertical,
                Spacing = 15
            };

            checkButton.Click += (_, _) =>
            {
                SizeManager.Reset();
                MessageBox.Show(T._("Die Änderungen werden beim nächsten Programmstart angewendet!"), "FPLedit");
            };

            toolbarCheckBox.Checked = pluginInterface.Settings.Get(NetworkEditingControl.TOOLBAR_ICON_SETTINGS_KEY, true);
            toolbarCheckBox.CheckedChanged += (_, _)
                => pluginInterface.Settings.Set(NetworkEditingControl.TOOLBAR_ICON_SETTINGS_KEY, toolbarCheckBox.Checked!.Value);

            return stack;
        }
    }
}
#nullable enable
using Eto.Drawing;
using Eto.Forms;
using FPLedit.Shared;
using FPLedit.Shared.UI;

namespace FPLedit.SettingsUi
{
    public class WindowSizeControl : ISettingsControl
    {
        public string DisplayName => T._("Fenstergrößen");

        public Control GetControl(IPluginInterface pluginInterface)
        {
#pragma warning disable CA2000
            var checkButton = new Button { Text = T._("Gespeicherte Fenstergrößen löschen") };
#pragma warning restore CA2000
            var stack = new StackLayout(checkButton)
            {
                Padding = new Padding(10),
                Orientation = Orientation.Vertical,
                Spacing = 5
            };

            checkButton.Click += (_, _) =>
            {
                SizeManager.Reset();
                MessageBox.Show(T._("Die Änderungen werden beim nächsten Programmstart angewendet!"), "FPLedit");
            };

            return stack;
        }
    }
}
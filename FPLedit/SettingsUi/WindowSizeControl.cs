using Eto.Drawing;
using Eto.Forms;
using FPLedit.Shared;
using FPLedit.Shared.UI;

namespace FPLedit.SettingsUi
{
    public class WindowSizeControl : ISettingsControl
    {
        public string DisplayName => "Fenstergrößen";

        public Control GetControl(IPluginInterface pluginInterface)
        {
#pragma warning disable CA2000
            var checkButton = new Button { Text = "Gespeicherte Fenstergrößen löschen" };
#pragma warning restore CA2000
            var stack = new StackLayout(checkButton)
            {
                Padding = new Padding(10),
                Orientation = Orientation.Vertical,
                Spacing = 5
            };

            checkButton.Click += (s, e) =>
            {
                SizeManager.Reset();
                MessageBox.Show("Die Änderungen werden beim nächsten Programmstart angewendet!", "FPLedit");
            };

            return stack;
        }
    }
}
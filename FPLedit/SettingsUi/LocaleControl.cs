#nullable enable
using Eto.Drawing;
using Eto.Forms;
using FPLedit.Shared;

namespace FPLedit.SettingsUi
{
    public class LocaleControl : ISettingsControl
    {
        public string DisplayName => T._("Sprache");

        public Control GetControl(IPluginInterface pluginInterface)
        {
            var availableLocales = T.GetAvailableLocales();
            var currentLocale = T.GetCurrentLocale();
            
            var stack = new StackLayout()
            {
                Padding = new Padding(10),
                Orientation = Orientation.Vertical,
                Spacing = 5
            };
            RadioButton? master = null;
            
            stack.Items.Add(new Label { Text = T._("Sprache der Benutzeroberfläche:") });

            foreach (var locale in availableLocales)
            {
                var rb = new RadioButton(master)
                {
                    Text = locale.Value,
                    Checked = locale.Key == currentLocale,
                };
                master ??= rb;

                rb.CheckedChanged += (_, _) =>
                {
                    if (rb.Checked)
                    {
                        pluginInterface.Settings.Set("lang", locale.Key);
                        MessageBox.Show(T._("Die Änderungen werden beim nächsten Programmstart angewendet!"), "FPLedit");
                    }
                };

                stack.Items.Add(rb);
            }

            return stack;
        }
    }
}
using System;
using Eto.Drawing;
using Eto.Forms;
using FPLedit.Shared;
using FPLedit.Shared.UI;

namespace FPLedit.SettingsUi
{
    public class AutomaticUpdateControl : ISettingsControl
    {
        public string DisplayName => LocalizationHelper._("Überprüfung auf neue Versionen");

        public Control GetControl(IPluginInterface pluginInterface)
        {
            var mg = new UpdateManager(pluginInterface.Settings);

#pragma warning disable CA2000
            var cb = new CheckBox { Text = LocalizationHelper._("Automatische Überprüfung auf Updates beim Programmstart aktivieren.") };
            var privacyTitle = new Label { Text = LocalizationHelper._("Datenschutz:"), Font = SystemFonts.Bold() };
            var label = new Label { Text = LocalizationHelper._("Dabei wird Ihre IP-Adresse und der verwendete Betriebssystemtyp an den Server übermittelt; Die IP-Adresse wird nur anonymisiert in Log-Dateien gespeichert; ein Rückschluss auf einzelne Benutzer ist daher nicht möglich.") };
            var checkButton = new Button { Text = LocalizationHelper._("Auf neue Version prüfen") };
#pragma warning restore CA2000
            var stack = new StackLayout(cb, privacyTitle, label, checkButton)
            {
                Padding = new Padding(10),
                Orientation = Orientation.Vertical,
                Spacing = 5
            };
            cb.CheckedBinding.Bind(() => mg.AutoUpdateEnabled, (b) => mg.AutoUpdateEnabled = b ?? false);

            checkButton.Click += (s, e) =>
            {
                mg.CheckResult = (newAvail, vi) =>
                {
                    if (newAvail)
                    {
                        DialogResult res = MessageBox.Show(LocalizationHelper._("Eine neue Programmversion ({0}) ist verfügbar!\n{1}\nJetzt zur Download-Seite wechseln, um die neue Version herunterzuladen?", vi.NewVersion, vi.Description ?? ""),
                            LocalizationHelper._("Neue FPLedit-Version verfügbar"), MessageBoxButtons.YesNo);

                        if (res == DialogResult.Yes)
                            OpenHelper.Open(vi.DownloadUrl);
                    }
                    else
                    {
                        MessageBox.Show(LocalizationHelper._("Sie benutzen bereits die aktuelle Version!"),
                            LocalizationHelper._("Auf neue Version prüfen"));
                    }
                };
                mg.CheckError = ex =>
                {
                    MessageBox.Show(LocalizationHelper._("Verbindung mit dem Server fehlgeschlagen!"),
                        LocalizationHelper._("Auf neue Version prüfen"));
                };

                mg.CheckAsync();
                checkButton.Enabled = false;
            };

            return stack;
        }
    }
}
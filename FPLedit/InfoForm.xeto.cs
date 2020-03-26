using Eto.Drawing;
using Eto.Forms;
using FPLedit.Shared;
using FPLedit.Shared.UI;
using System;
using System.IO;
using FPLedit.Shared.Helpers;

namespace FPLedit
{
    internal sealed class InfoForm : FDialog
    {
#pragma warning disable CS0649
        private readonly TextArea licenseTextArea;
        private readonly Label versionLabel, privacyLabel;
        private readonly CheckBox updateCheckBox;
        private readonly Button checkButton;
#pragma warning restore CS0649

        private readonly UpdateManager mg;

        public InfoForm(ISettings settings)
        {
            Eto.Serialization.Xaml.XamlReader.Load(this);

            mg = new UpdateManager(settings);

            using (var stream = this.GetResource("Resources.Info.txt"))
            using (var sr = new StreamReader(stream))
                licenseTextArea.Text = sr.ReadToEnd();

            versionLabel.Text = versionLabel.Text.Replace("{version}", VersionInformation.Current.DisplayVersion);
            versionLabel.Font = new Font(versionLabel.Font.FamilyName, versionLabel.Font.Size, FontStyle.Bold);
            updateCheckBox.Checked = mg.AutoUpdateEnabled;
            
            privacyLabel.WordWrap(430);
        }

        private void VersionCheck()
        {
            mg.CheckResult = (newAvail, vi) =>
            {
                if (newAvail)
                {
                    string nl = Environment.NewLine;
                    DialogResult res = MessageBox.Show($"Eine neue Programmversion ({vi.NewVersion}) ist verfügbar!{nl}{vi.Description ?? ""}{nl}Jetzt zur Download-Seite wechseln, um die neue Version herunterzuladen?",
                        "Neue FPLedit-Version verfügbar", MessageBoxButtons.YesNo);

                    if (res == DialogResult.Yes)
                        OpenHelper.Open(vi.DownloadUrl);
                }
                else
                {
                    MessageBox.Show($"Sie benutzen bereits die aktuelle Version!",
                        "Auf neue Version prüfen");
                }
            };
            mg.CheckError = ex =>
            {
                MessageBox.Show($"Verbindung mit dem Server fehlgeschlagen!",
                    "Auf neue Version prüfen");
            };

            mg.CheckAsync();
            checkButton.Enabled = false;
        }

        private void CheckButton_Click(object sender, EventArgs e)
            => VersionCheck();

        private void CloseButton_Click(object sender, EventArgs e)
        {
            mg.AutoUpdateEnabled = updateCheckBox.Checked.Value;
            Close();
        }
    }
}

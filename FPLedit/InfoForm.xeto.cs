using Eto.Drawing;
using Eto.Forms;
using FPLedit.Shared;
using FPLedit.Shared.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPLedit
{
    internal class InfoForm : Dialog
    {
#pragma warning disable CS0649
        private readonly TextArea licenseTextArea;
        private readonly Label versionLabel;
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

            versionLabel.Text = versionLabel.Text.Replace("{version}", mg.CurrentVersion.ToString());
            versionLabel.Font = new Font(versionLabel.Font.FamilyName, versionLabel.Font.Size, FontStyle.Bold);
            updateCheckBox.Checked = mg.AutoUpdateEnabled;
        }

        private void VersionCheck()
        {
            mg.CheckResult = (new_avail, vi) =>
            {
                if (new_avail)
                {
                    string nl = Environment.NewLine;
                    DialogResult res = MessageBox.Show($"Eine neue Programmversion ({vi.NewVersion.ToString()}) ist verfügbar!{nl}{vi.Description ?? ""}{nl}Jetzt zur Download-Seite wechseln, um die neue Version herunterzuladen?",
                        "Neue FPLedit-Version verfügbar", MessageBoxButtons.YesNo);

                    if (res == DialogResult.Yes)
                        Process.Start(vi.DownloadUrl);
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

        private void docuLink_Click(object sender, EventArgs e)
            => Process.Start("https://fahrplan.manuelhu.de/");

        private void checkButton_Click(object sender, EventArgs e)
            => VersionCheck();

        private void closeButton_Click(object sender, EventArgs e)
        {
            mg.AutoUpdateEnabled = updateCheckBox.Checked.Value;
            Close();
        }
    }
}
